using System;
using System.Collections.Generic;

namespace ChatServer.Models.Entities
{
    public partial class FileUpload
    {
        public FileUpload()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Path { get; set; }
        public long? Size { get; set; }
        public DateTime? UploadedAt { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
