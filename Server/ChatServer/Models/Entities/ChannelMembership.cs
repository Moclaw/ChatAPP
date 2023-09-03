using System;
using System.Collections.Generic;

namespace ChatServer.Models.Entities
{
    public partial class ChannelMembership
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? ChannelId { get; set; }
        
		[Newtonsoft.Json.JsonIgnore]
        public virtual Channel? Channel { get; set; }
		[Newtonsoft.Json.JsonIgnore]
        public virtual User? User { get; set; }
    }
}
