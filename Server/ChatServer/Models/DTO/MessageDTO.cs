namespace ChatServer.Models.DTO
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public int ChannelId { get; set; } 
        public string Type { get; set; } = "";
        public int UserId { get; set; }
        public string Topic { get; set; } = "";
        public string SenderStatus { get; set; } = "";
        public string Username { get; set; } = "";
        public string File { get; set; } = "";
       
        public DateTime dateTime { get; set; }
        public string CreatedAt
        {
            get
            {
                var timeSpan = DateTime.Now.Subtract(dateTime);
                if (timeSpan.TotalDays > 365)
                {
                    return $"{Math.Round(timeSpan.TotalDays / 365)} year ago";
                }
                else if (timeSpan.TotalDays > 30)
                {
                    return $"{Math.Round(timeSpan.TotalDays / 30)} month ago";
                }
                else if (timeSpan.TotalDays > 7)
                {
                    return $"{Math.Round(timeSpan.TotalDays / 7)} week ago";
                }
                else if (timeSpan.TotalDays > 1)
                {
                    return $"{Math.Round(timeSpan.TotalDays)} day ago";
                }
                else if (timeSpan.TotalHours > 1)
                {
                    return $"{Math.Round(timeSpan.TotalHours)} hour ago";
                }
                else if (timeSpan.TotalMinutes > 1)
                {
                    return $"{Math.Round(timeSpan.TotalMinutes)} minute ago";
                }
                else
                {
                    return $"{Math.Round(timeSpan.TotalSeconds)} second ago";
                }
            }
           
        }
    }
}
