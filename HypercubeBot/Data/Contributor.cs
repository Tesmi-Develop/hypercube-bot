using System.Text.Json.Serialization;

namespace HypercubeBot.Data;

[Serializable]
public struct Contributor
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("html_url")]
    public string Url { get; set; }
}