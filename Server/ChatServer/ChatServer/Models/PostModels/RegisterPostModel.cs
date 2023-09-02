namespace ChatServer.Models.PostModels
{
    public class RegisterPostModel
    {
        public string? Password { get; set; } = null!;
        public string? ConfirmPassword { get; set; } = null!;
        public string? Username { get; set; } = null!;
        public int? FileId { get; set; } = null!;
    }
}
