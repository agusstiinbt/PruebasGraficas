namespace CigoWeb.Core.Models.Geocoding;

public class GeocodingCoordinateValidationResult
{
    public bool IsValid =>
    !IsLatitudeMissing &&
    !IsLongitudeMissing &&
    !IsLatitudeInvalid &&
    !IsLongitudeInvalid;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsLatitudeMissing { get; set; }
    public bool IsLongitudeMissing { get; set; }

    public bool IsLatitudeInvalid { get; set; }
    public bool IsLongitudeInvalid { get; set; }
}
