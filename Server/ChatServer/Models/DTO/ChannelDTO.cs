namespace ChatServer.Models.DTO
{
    public class ChannelDTO
    {
        public int Id { get; set; }
        public string Topic { get; set; } = "";
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public bool UnRead { get; set; } = true;
    }
    public class NotificationDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
    }
}
 