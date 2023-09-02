using ChatServer.Data;
using ChatServer.Models.Entities;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ChatServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ProducerConfig _producerConfig;
        private readonly IHubContext<ChatHub> _clients;

        public ChatHub(ProducerConfig producerConfig, IHubContext<ChatHub> clients)
        {
            _producerConfig = producerConfig;
            _clients = clients;
        }

        public ProducerConfig GetProducer()
        {
            return _producerConfig;
        }

        public async Task SendMessage(Message message)
        {
            try
            {
                await _clients.Clients.All.SendAsync("ReceiveMessage", message);
                using var producer = new ProducerBuilder<Null, string>(_producerConfig)
                    .Build();

                var result = await producer.ProduceAsync(message?.Channel?.Name, new Message<Null, string>
                {
                    Value = JsonConvert.SerializeObject(message)
                });
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
