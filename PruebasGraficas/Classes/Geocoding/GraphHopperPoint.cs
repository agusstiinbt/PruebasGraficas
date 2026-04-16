using System.Text.Json.Serialization;

namespace CigoWeb.Core.Models.Geocoding;

public sealed class GraphHopperPoint
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}
