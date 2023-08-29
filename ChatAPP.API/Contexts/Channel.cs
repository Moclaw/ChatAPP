using System;
using System.Collections.Generic;

namespace ChatAPP.API.Contexts
{
    public partial class Channel
    {
        public Channel()
        {
            ChannelMemberships = new HashSet<ChannelMembership>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<ChannelMembership> ChannelMemberships { get; set; }
    }
}
