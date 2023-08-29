﻿using System;
using System.Collections.Generic;

namespace ChatAPP.API.Contexts
{
    public partial class Notification
    {
        public int Id { get; set; }
        public int? MessageId { get; set; }
        public int? UserId { get; set; }
        public bool IsRead { get; set; }

        public virtual Message? Message { get; set; }
        public virtual User? User { get; set; }
    }
}
