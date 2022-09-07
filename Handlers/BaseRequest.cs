using Newtonsoft.Json;

namespace mediatr_todos;

public class BaseRequest
{
    [JsonIgnore]public int UserId { get; set; }
}