using System;
using System.Collections.Generic;

namespace ChatAPP.API.Contexts
{
    public partial class Message
    {
        public Message()
        {
            Notifications = new HashSet<Notification>();
        }

        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int? UserId { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
