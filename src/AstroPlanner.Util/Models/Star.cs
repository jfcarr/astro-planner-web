using System.Text.Json.Serialization;

namespace AstroPlanner.Util.Models;

public class Star
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("rightAscension")]
    public double RightAscension { get; set; }

    [JsonPropertyName("declination")]
    public double Declination { get; set; }
}
