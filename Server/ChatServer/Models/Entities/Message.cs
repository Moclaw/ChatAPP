using System;
using System.Collections.Generic;

namespace ChatServer.Models.Entities
{
    public partial class Message
    {
        public Message()
        {
            Notifications = new HashSet<Notification>();
        }

        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public int? UserId { get; set; }
        public string? SenderStatus { get; set; }
        public DateTime? SendTime { get; set; }
        public int? ChannelId { get; set; }

		[Newtonsoft.Json.JsonIgnore]
        public virtual Channel? Channel { get; set; }
		[Newtonsoft.Json.JsonIgnore]
        public virtual User? User { get; set; }
		[Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<Notification> Notifications { get; set; }
    }

    public enum SenderStatusEnum
    {
        Sent = 1,
        Delivery = 3,
        Seen = 2
    }

    public enum ReceiverStatusEnum
    {
        Received = 1,
        NotReceived = 2,
        Seen = 3
    }

    public enum MessageTypeEnum
    {
        Text = 1,
        Image = 2,
        Video = 3,
        File = 4
    }

}
