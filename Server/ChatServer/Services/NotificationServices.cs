using System;
using ChatServer.Data;
using ChatServer.Models.Entities;

namespace ChatServer.Services
{
    public class NotificationServices
    {   
        private readonly ChatAPPContext _context;
        private readonly ILogger<NotificationServices> _logger; 
        public NotificationServices( ChatAPPContext context, ILogger<NotificationServices> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Notification>? GetNotifications(int userId)
        {
            try
            {
                var notifications = _context.Notifications.Where(n => n.UserId == userId).ToList();
                return notifications;

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

