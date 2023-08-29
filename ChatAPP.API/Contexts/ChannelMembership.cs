using System;
using System.Collections.Generic;

namespace ChatAPP.API.Contexts
{
    public partial class ChannelMembership
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? ChannelId { get; set; }

        public virtual Channel? Channel { get; set; }
        public virtual User? User { get; set; }
    }
}
