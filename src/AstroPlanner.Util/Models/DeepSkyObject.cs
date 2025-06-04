using System.Text.Json.Serialization;

namespace AstroPlanner.Util.Models;

public class DeepSkyObject
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("rightAscensionHours")]
    public double RightAscensionHours { get; set; }

    [JsonPropertyName("rightAscensionMinutes")]
    public double RightAscensionMinutes { get; set; }

    [JsonPropertyName("rightAscensionSeconds")]
    public double RightAscensionSeconds { get; set; }

    [JsonPropertyName("declinationDegrees")]
    public double DeclinationDegrees { get; set; }

    [JsonPropertyName("declinationMinutes")]
    public double DeclinationMinutes { get; set; }

    [JsonPropertyName("declinationSeconds")]
    public double DeclinationSeconds { get; set; }

    [JsonPropertyName("constellation")]
    public string? Constellation { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("magnitude")]
    public double Magnitude { get; set; }
}
