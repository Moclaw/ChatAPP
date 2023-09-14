using ChatServer.Data;
using ChatServer.Models.DTO;
using ChatServer.Models.Entities;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Channels;

namespace ChatServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHubContext<ChatHub> _clients;
        private readonly IProducer<string, string> producer;
        private readonly IServiceScopeFactory _serviceScope;

        public ChatHub(
            IHubContext<ChatHub> clients,
            IProducer<string, string> producer,
            IServiceScopeFactory serviceScope
        )
        {
            _clients = clients;
            this.producer = producer;
            _serviceScope = serviceScope;
        }

        public async Task JoinChannel(string topic)
        {
            await _clients.Groups.AddToGroupAsync(Context.ConnectionId, topic);
            await _clients.Clients.All.SendAsync("ChannelMessage", topic);
        }

        public async Task LeaveChannel(string topic)
        {
            await _clients.Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
        }

        public async Task SendNoitification(NotificationDTO notification)
        {
            try
            {
                await _clients.Clients
                    .Group(notification.Topic)
                    .SendAsync("Notification", notification);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task SendMessage(MessageDTO message, string topic)
        {
            try
            {
                message.SenderStatus = SenderStatusEnum.Sent.ToString();
                using var scope = _serviceScope.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ChatAPPContext>();
                var mess = await context.Messages.FirstOrDefaultAsync(c => c.Id == message.Id);
                mess!.SenderStatus = message.SenderStatus;
                context.Messages.Update(mess);
                await context.SaveChangesAsync();

                string json = JsonConvert.SerializeObject(
                    message,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }
                );
                var result = await producer.ProduceAsync(
                    topic,
                    new Message<string, string> { Key = message.UserId.ToString(), Value = json }
                );
                if (result != null)
                {
                    await _clients.Clients.Group(topic).SendAsync("Message", message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task DeleteChannel(ChannelDTO channel)
        {
            try
            {
                await _clients.Clients.All.SendAsync("DeleteChannel", channel);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await _clients.Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }
}
