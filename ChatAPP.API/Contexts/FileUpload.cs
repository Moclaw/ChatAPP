using System;
using System.Collections.Generic;

namespace ChatAPP.API.Contexts
{
    public partial class FileUpload
    {
        public FileUpload()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Path { get; set; } = null!;
        public int Size { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
