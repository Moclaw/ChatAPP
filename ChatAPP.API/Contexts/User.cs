﻿using System;
using System.Collections.Generic;

namespace ChatAPP.API.Contexts
{
    public partial class User
    {
        public User()
        {
            ChannelMemberships = new HashSet<ChannelMembership>();
            Messages = new HashSet<Message>();
            Notifications = new HashSet<Notification>();
        }

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int? FileId { get; set; }

        public virtual FileUpload? File { get; set; }
        public virtual ICollection<ChannelMembership> ChannelMemberships { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
