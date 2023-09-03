
using System.Text.Json.Serialization;
public class DefaultResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }
    public string? Message { get; set; } = null!;

}