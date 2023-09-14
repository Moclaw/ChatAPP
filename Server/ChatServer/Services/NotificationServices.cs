using System;
using System.Net.WebSockets;
using ChatServer.Data;
using ChatServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Services
{
    public class NotificationServices
    {
        private readonly ChatAPPContext _context;
        private readonly ILogger<NotificationServices> _logger;

        public NotificationServices(ChatAPPContext context, ILogger<NotificationServices> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<object>? GetNotifications(int userId)
        {
            try
            {
                var result = new List<object>();
                var messages = _context.Messages
                    .AsNoTracking()
                    .Where(c => c.UserId != userId)
                    .ToList();
                var channels = _context.Channels.AsNoTracking().ToList();
                var users = _context.Users.AsNoTracking().ToList();
                foreach (var i in channels)
                {
                    var message = messages.Where(c => c.ChannelId == i.Id).ToList();
                    foreach (var m in message)
                    {
                        if (m.SenderStatus == SenderStatusEnum.Sent.ToString())
                        {
                            var user = users.Where(c => c.Id == m.UserId).FirstOrDefault();
                            result.Add(
                                new
                                {
                                    Id = m.Id,
                                    Sender = user!.Username,
                                    Content = m.Content,
                                    Channel = i.Name,
                                    IsRead = false,
                                    CreatedAt = m.SendTime
                                }
                            );
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public DefaultResponse ReadNotification(int id)
        {
            try
            {
                var notification = _context.Notifications.Where(n => n.Id == id).FirstOrDefault();
                if (notification == null)
                {
                    return new DefaultResponse { Message = "Notification not found" };
                }
                notification.IsRead = true;
                _context.SaveChanges();
                return new DefaultResponse { Message = "Read notification success" };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new DefaultResponse { Message = "Read notification failed" };
            }
        }

        public DefaultResponse ReadAllNotifications(int userId)
        {
            try
            {
                var notifications = _context.Notifications.Where(n => n.UserId == userId).ToList();
                foreach (var item in notifications)
                {
                    item.IsRead = true;
                }
                _context.SaveChanges();
                return new DefaultResponse { Message = "Read all notifications success" };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new DefaultResponse { Message = "Read all notifications failed" };
            }
        }
    }
}
