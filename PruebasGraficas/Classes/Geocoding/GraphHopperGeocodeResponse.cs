using System.Text.Json.Serialization;

namespace CigoWeb.Core.Models.Geocoding;

public sealed class GraphHopperGeocodeResponse
{
    [JsonPropertyName("hits")]
    public List<GraphHopperGeocodeHit>? Hits { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
