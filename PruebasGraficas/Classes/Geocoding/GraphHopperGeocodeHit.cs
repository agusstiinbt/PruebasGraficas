using System.Text.Json.Serialization;

namespace CigoWeb.Core.Models.Geocoding;

public sealed class GraphHopperGeocodeHit
{
    [JsonPropertyName("point")]
    public GraphHopperPoint? Point { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("housenumber")]
    public string? HouseNumber { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }
}
