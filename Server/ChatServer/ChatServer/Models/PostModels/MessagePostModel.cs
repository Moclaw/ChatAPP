namespace ChatServer.Models.PostModels
{
    public class MessagePostModel
    {
		public string? Content { get; set; }
		public string? Type { get; set; }
		public int? UserId { get; set; }
		public DateTime? SendTime { get; set; } = DateTime.Now;
    }
}
