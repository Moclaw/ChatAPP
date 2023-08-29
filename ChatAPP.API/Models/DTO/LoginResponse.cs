namespace ChatAPP.API.Models.DTO
{
    public class LoginResponse
    {
        public string? Token { get; set; } = null!;
        public string? Message { get; set; } = null!;
        public object? Data { get; set; }
    }
}
