namespace ChatServer.Models.PostModels
{
    public class ChannelPostModel
    {
		public string? Name { get; set; }    
		public int UserId { get; set; } 
		public List<int> AssignMember { get; set; } = new List<int>();
    }
}
