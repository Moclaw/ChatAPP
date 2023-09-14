using ChatServer.Data;
using ChatServer.Hubs;
using ChatServer.Models.DTO;
using ChatServer.Models.Entities;
using ChatServer.Models.PostModels;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace ChatServer.Services;

public class ChannelServices
{
    private readonly ChatAPPContext _context;
    private readonly ILogger<ChannelServices> _logger;
    private readonly AdminClientBuilder _adminClientBuilder;
    private readonly ChatHub _chatHub;
    private readonly AwsSNSServices _awsSNSServices;

    public ChannelServices(ChatAPPContext context, ILogger<ChannelServices> logger,
        IConfiguration configuration, ChatHub chatHub, AwsSNSServices awsSNSServices)
    {
        _context = context;
        _logger = logger;
        var config = new AdminClientConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        _adminClientBuilder = new AdminClientBuilder(config);
        _chatHub = chatHub;
        _awsSNSServices = awsSNSServices;
    }

    public async Task<DefaultResponse> CreateChannel(ChannelPostModel model, int currentId)
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
            var channel = new Models.Entities.Channel
            {
                Name = model.Name,
                Topic = topic
            };

            _context.Channels.Add(channel);
            _context.SaveChanges();
            model.AssignMember.Add(model.UserId);
            var assginUser = _context.Users
                .AsNoTracking()
                .Where(c => model.AssignMember.Contains(c.Id)).ToList();
            var user = _context.Users.Where(c => c.Id == model.UserId).FirstOrDefault();
            await _awsSNSServices.SendNotification(new NotificationDTO
            {
                Title = "New channel",
                Body = $"{user?.Username} created new channel {model.Name}",
                Topic = topic,
            });
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
                .AsNoTracking()
                .Include(c => c.ChannelMemberships)
                .Where(c => c.Id == channel.Id)
                .ToList();

            await _chatHub.JoinChannel(topic);



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

    public async Task<DefaultResponse> DeleteChannel(int id)
    {
        try
        {
            if (id == 0)
            {
                var channels = _context.Channels.AsNoTracking()
                    .Include(c => c.ChannelMemberships)
                    .Include(c => c.Messages)
                    .ToList();
                _context.Channels.RemoveRange(channels);
                _context.SaveChanges();
                return new DefaultResponse { Message = "Delete all channel success" };
            }
            var channel = _context.Channels.Where(x => x.Id == id).AsNoTracking()
                    .Include(c => c.ChannelMemberships)
                    .Include(c => c.Messages).FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }
            try
            {
                //unsubcribe topic
                _adminClientBuilder.Build().DeleteTopicsAsync(new List<string> { channel?.Topic! }).GetAwaiter().GetResult();
                await _awsSNSServices.Delete(channel!.Topic!);
            }
            catch (Exception)
            {
                //continues 
            }
            await _chatHub.DeleteChannel(new ChannelDTO
            {
                Id = channel!.Id,
                Name = channel.Name!,
                Topic = channel.Topic!,
            });

            _context.Channels.Remove(channel!);

            _context.SaveChanges();
            return new DefaultResponse { Message = "Delete channel success", Data = new { Ok = "OK" } };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Delete channel failed" };
        }
    }

    public async Task<DefaultResponse> SendMessage(int id, MessagePostModel model, int currentId)
    {
        try
        {
            var channel = _context.Channels.Where(x => x.Id == id)
                .AsNoTracking()
                .FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }

            var message = new Message
            {
                Content = model.Content,
                Type = model.Type,
                ChannelId = id,
                UserId = currentId,
                SendTime = DateTime.Now,
                SenderStatus = SenderStatusEnum.Delivery.ToString(),
            };
            var members = _context.ChannelMemberships.AsNoTracking()
                .Where(x => x.ChannelId == id && x.UserId != currentId).ToList();
            foreach (var item in members)
            {
                var notification = new Notification
                {
                    UserId = item.UserId,
                    IsRead = false,
                };
                message.Notifications.Add(notification);
            }

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var send = new MessageDTO
            {
                Id = message.Id,
                Content = message.Content!,
                SenderStatus = message.SenderStatus!,
                Type = message.Type!,
                UserId = message.UserId!.Value,
                ChannelId = message.ChannelId!.Value,
                Username = message.User?.Username!,
                dateTime = message.SendTime!.Value,
            };
            await _chatHub.JoinChannel(channel.Topic!);

            await _chatHub.SendMessage(send, channel.Topic!);

            return new DefaultResponse { Message = "Send message success", Data = send };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Send message failed" };
        }

    }

    public DefaultResponse GetChannelsByUserId(int userId)
    {
        try
        {
            var channels = _context.Channels.AsNoTracking()
                .Include(c => c.ChannelMemberships)
                .Where(c => c.ChannelMemberships.Any(x => x.UserId == userId))
                .ToList();

            if (channels == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }

            var result = new List<ChannelDTO>();
            var messages = _context.Messages.ToList();

            foreach (var item in channels)
            {
                var mess = messages.Where(x => x.ChannelId == item.Id).ToList();
                if (mess == null)
                    continue;
                result.Add(new ChannelDTO
                {
                    Id = item.Id,
                    Name = item.Name!,
                    Topic = item.Topic!,
                    UserId = userId,
                    UnRead = mess.Any(x => x.UserId != userId && x.SenderStatus != SenderStatusEnum.Sent.ToString()) ? true : false,
                });
            }

            return new DefaultResponse { Message = "Get channels success", Data = result, Count = channels.Count };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Get channels failed" };
        }
    }

    public async Task<DefaultResponse> UpdateChannelStatus(int channelId, int userId)
    {
        var messages = _context.Messages.Where(x => x.ChannelId == channelId && x.UserId != userId).ToList();
        foreach (var i in messages)
        {
            if (i.SenderStatus == SenderStatusEnum.Sent.ToString())
            {
                i.SenderStatus = SenderStatusEnum.Seen.ToString();
            }
        }
        _context.Messages.UpdateRange(messages);
        await _context.SaveChangesAsync();
        var channel = _context.Channels.AsNoTracking()
            .Where(x => x.Id == channelId).FirstOrDefault();
        return new DefaultResponse { Message = "Update channel status success", Data = new MessageDTO() };
    }

    public async Task<DefaultResponse> GetChannelMessagesByUserId(int channelId, int userId)
    {
        try
        {
            var channel = _context.Channels.Where(x => x.Id == channelId).FirstOrDefault();
            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }
            await _chatHub.JoinChannel(channel.Topic!);

            var messages = await _context.Messages.AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.ChannelId == channelId)
            .ToListAsync();

            if (messages == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }

            var notificationIds = messages.Select(x => x.Id).ToList();
            var notifications = _context.Notifications.AsNoTracking().Where(x => notificationIds.Contains(x.MessageId!.Value) && x.UserId == userId).ToList();
            foreach (var item in notifications)
            {
                item.IsRead = true;
            }
            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();

            var result = new List<MessageDTO>();
            foreach (var item in messages)
            {
                var mess = new MessageDTO
                {
                    Id = item.Id,
                    Content = item.Content!,
                    SenderStatus = item.SenderStatus!,
                    Type = item.Type!,
                    UserId = item.UserId!.Value,
                    ChannelId = item.ChannelId!.Value,
                    Topic = channel.Topic!,
                    File = item.User?.File?.Path!,
                    Username = item.User?.Username!,
                    dateTime = item.SendTime!.Value,
                };
                result.Add(mess);
            }
            result = result.OrderBy(x => x.dateTime).ToList();

            return new DefaultResponse { Message = "Get channel messages success", Data = result, Count = result.Count };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Get channel messages failed" };
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
            var channel = _context.Channels.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();
            var members = _context.ChannelMemberships.AsNoTracking()
                .Where(x => x.ChannelId == id)
                .ToList();

            var users = _context.Users
                .Include(x => x.File)
                .AsEnumerable()
                .Where(x => members.Any(c => c.UserId!.Value == x.Id))
                .ToList();


            if (channel == null)
            {
                return new DefaultResponse { Message = "Channel not found" };
            }

            var result = new
            {
                Users = users,
                Channel = channel
            };
            return new DefaultResponse { Message = "Get channel success", Data = result };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new DefaultResponse { Message = "Get channel failed" };
        }
    }

    public List<MessageDTO>? GetChannelMessages(int id, int userId)
    {
        try
        {
            var messages = _context.Messages.AsNoTracking().Where(x => x.ChannelId == id).ToList();
            if (messages == null)
            {
                return null;
            }

            var messageIds = messages.Select(x => x.Id).ToList();
            var notifications = _context.Notifications.AsNoTracking().Where(x => messageIds.Contains(x.MessageId!.Value) && x.UserId == userId).ToList();
            foreach (var item in notifications)
            {
                item.IsRead = true;
            }

            _context.SaveChanges();
            messages = _context.Messages.AsNoTracking().Where(x => x.ChannelId == id).ToList();
            var result = new List<MessageDTO>();
            foreach (var item in messages)
            {
                var mess = new MessageDTO
                {
                    Id = item.Id,
                    Content = item.Content!,
                    SenderStatus = item.SenderStatus!,
                    Type = item.Type!,
                    UserId = item.UserId!.Value,
                    ChannelId = item.ChannelId!.Value,
                    dateTime = item.SendTime!.Value,
                };
                result.Add(mess);
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
}