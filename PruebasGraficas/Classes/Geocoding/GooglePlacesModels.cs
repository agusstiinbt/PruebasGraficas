using System.Text.Json.Serialization;
using CigoWeb.Core.Helpers.Address;

namespace CigoWeb.Core.Models.Geocoding;

public class PlacePrediction
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("place_id")]
    public string PlaceId { get; set; } = string.Empty;
}

public class PlacesAutocompleteResponse
{
    [JsonPropertyName("predictions")]
    public List<PlacePrediction>? Predictions { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

public class PlaceDetailsResponse
{
    [JsonPropertyName("result")]
    public PlaceResult? Result { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

public class PlaceResult
{
    [JsonPropertyName("address_components")]
    public List<AddressComponent>? AddressComponents { get; set; }

    [JsonPropertyName("geometry")]
    public PlaceGeometry? Geometry { get; set; }
}

public class PlaceGeometry
{
    [JsonPropertyName("location")]
    public PlaceLocation? Location { get; set; }
}

public class PlaceLocation
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}
