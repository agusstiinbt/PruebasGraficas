using System.Text.Json.Serialization;
using CigoWeb.Core.Helpers.Address;

namespace CigoWeb.Core.Models.Geocoding;

public class GeocodingResult
{
    [JsonPropertyName("address_components")]
    public List<AddressComponent>? AddressComponents { get; set; }

    [JsonPropertyName("formatted_address")]
    public string FormattedAddress { get; set; } = string.Empty;
}
