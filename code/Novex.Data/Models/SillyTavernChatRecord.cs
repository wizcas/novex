using System.Text.Json.Serialization;

namespace Novex.Data.Models;

public class SillyTavernChatRecord
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mes")]
    public string? Mes { get; set; }

    [JsonPropertyName("send_date")]
    public string? SendDate { get; set; }
}
