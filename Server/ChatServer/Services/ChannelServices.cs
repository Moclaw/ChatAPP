
using ChatServer.Data;
using ChatServer.Hubs;
using ChatServer.Models.Entities;
using ChatServer.Models.PostModels;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatServer.Services;

public class ChannelServices
{
    private readonly ChatAPPContext _context;
    private readonly ILogger<ChannelServices> _logger;
    private readonly AdminClientBuilder _adminClientBuilder;
    private readonly ChatHub _chatHub;
    public ChannelServices(ChatAPPContext context, ILogger<ChannelServices> logger, IConfiguration configuration, ChatHub chatHub)
    {
        _context = context;
        _logger = logger;
        var config = new AdminClientConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        _adminClientBuilder = new AdminClientBuilder(config);
        _chatHub = chatHub;
    }

    public async Task<DefaultResponse> CreateChannel(ChannelPostModel model)
    {
        try
        {
            var topic = Guid.NewGuid().ToString();
            await _adminClientBuilder.Build().CreateTopicsAsync(new List<TopicSpecification>
            {
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1,
                }
            });

            var message = new Message
            {
                Content = "Welcome to " + model.Name + " channel",
                SenderStatus = SenderStatusEnum.Sent.ToString(),
                SendTime = DateTime.Now,
                Type = MessageTypeEnum.Text.ToString(),
                UserId = model.UserId,
            };
            var channel = new Channel
            {
                Name = model.Name,
                Messages = new List<Message>()
                {
                    message
                },
                Topic = topic
            };

            _context.Channels.Add(channel);
            _context.SaveChanges();
            model.AssignMember.Add(model.UserId);
            var assginUser = _context.Users.Where(c => model.AssignMember.Contains(c.Id)).ToList();
            var user = _context.Users.Where(c => c.Id == model.UserId).FirstOrDefault();
            var notifications = new List<Notification>();
            foreach (var item in assginUser)
            {
                var notification = new Notification
                {
                    UserId = item.Id,
                    Message = channel?.Messages?.FirstOrDefault(),
                    IsRead = false,
                };
                notifications.Add(notification);
            }
            _context.Notifications.AddRange(notifications);
            _context.SaveChanges();

            var channelMemberships = new List<ChannelMembership>();
            foreach (var item in assginUser)
            {
                var channelMembership = new ChannelMembership
                {
                    UserId = item.Id,
                    Channel = channel,
                };
                channelMemberships.Add(channelMembership);
            }
            channelMemberships.Add(new ChannelMembership
            {
                UserId = model.UserId,
                Channel = channel
            });

            _context.ChannelMemberships.AddRange(channelMemberships);
            _context.SaveChanges();

            var result = _context.Channels
                .Include(c => c.ChannelMemberships)
                .Where(c => c.Id == channel.Id)
                .ToList();
            await _chatHub.SendMessage(message);

            return new DefaultResponse { Message = "Create channel success", Data = result };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Create channel failed" };
        }
    }

    public DefaultResponse UpdateChannel(int id, ChannelPostModel model)
    {
        try
        {
            var channel = _context.Channels.Where(x => x.Id == id).FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }
            channel.Name = model.Name;
            _context.SaveChanges();
            return new DefaultResponse { Message = "Update channel success", Data = channel };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Update channel failed" };
        }
    }

    public DefaultResponse DeleteChannel(int id)
    {
        try
        {
            var channel = _context.Channels.Where(x => x.Id == id).FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }
            _adminClientBuilder.Build().DeleteTopicsAsync(new List<string> { channel?.Topic! }).GetAwaiter().GetResult();
            _context.Channels.Remove(channel!);
            _context.SaveChanges();
            return new DefaultResponse { Message = "Delete channel success" };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Delete channel failed" };
        }
    }

    public async Task<DefaultResponse> SendMessage(int id, MessagePostModel model)
    {
        try
        {
            var channel = _context.Channels.Where(x => x.Id == id).FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }
            var message = new Message
            {
                Content = model.Content,
                SenderStatus = SenderStatusEnum.Sent.ToString(),
                SendTime = DateTime.Now,
                Type = model.Type,
                UserId = model.UserId,
                Channel = channel
            };
            _context.Messages.Add(message);
            _context.SaveChanges();

            var result = new Message
            {
                Id = message.Id,
                Content = message.Content,
                SenderStatus = message.SenderStatus,
                SendTime = message.SendTime,
                Type = message.Type,
                UserId = message.UserId,
                ChannelId = message.ChannelId,
                ReceiverStatus = message.ReceiverStatus,
                Channel = channel
            };

			await _chatHub.SendMessage(result);

            return new DefaultResponse { Message = "Send message success", Data = result };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Send message failed" };
        }

    }

    public DefaultResponse GetChannels()
    {
        try
        {
            var channels = _context.Channels.ToList();
            return new DefaultResponse { Message = "Get channels success", Data = channels };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Get channels failed" };
        }
    }

    public DefaultResponse GetChannel(int id)
    {
        try
        {
            var channel = _context.Channels.Where(x => x.Id == id).FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }
            return new DefaultResponse { Message = "Get channel success", Data = channel };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Get channel failed" };
        }
    }

    public List<Message>? GetChannelMessages(int id, int userId)
    {
        try
        {
            var messages = _context.Messages.Where(x => x.ChannelId == id).ToList();
            if (messages == null)
            {
                return null;
            }

            var messageIds = messages.Select(x => x.Id).ToList();
            var notifications = _context.Notifications.Where(x => messageIds.Contains(x.MessageId!.Value) && x.UserId == userId).ToList();
            foreach (var item in notifications)
            {
                item.IsRead = true;
            }
            var messageByUser = _context.Messages.Where(x => x.UserId == userId && x.ChannelId == id).ToList();
            foreach (var item in messageByUser)
            {
                item.ReceiverStatus = ReceiverStatusEnum.Received.ToString();
            }
            _context.Messages.UpdateRange(messageByUser);
            _context.SaveChanges();

            return messages;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
}