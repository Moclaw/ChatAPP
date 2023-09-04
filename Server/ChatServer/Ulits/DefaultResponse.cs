
using Newtonsoft.Json;

public class DefaultResponse
{
    public DefaultResponse()
    {
        Message = "";
        Data = null!;
    }
    public string Message { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object Data { get; set; }

}