using System.Text.Json.Serialization;

namespace CigoWeb.Core.Models.Geocoding;

public sealed class GraphHopperGeocodeRequest
{
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("reverse")]
    public bool? Reverse { get; set; }

    [JsonPropertyName("point_lat")]
    public double? PointLat { get; set; }

    [JsonPropertyName("point_lon")]
    public double? PointLon { get; set; }
}
