using System;
using System.Collections.Generic;

namespace ChatServer.Models.Entities
{
    public partial class Channel
    {
        public Channel()
        {
            ChannelMemberships = new HashSet<ChannelMembership>();
            Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Topic { get; set; }

		[Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<ChannelMembership> ChannelMemberships { get; set; }
		[Newtonsoft.Json.JsonIgnore]
		public virtual ICollection<Message> Messages { get; set; }
    }
}
