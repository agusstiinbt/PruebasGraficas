using CigoWeb.Core.Helpers.Address;
using PruebasGraficas.Classes.Models.Address;

namespace CigoWeb.Core.Helpers;

public static class AddressHelper
{
    /// <summary>
    /// Formats an <see cref="AddressResponse"/> into a single comma-separated display string.
    /// Returns "-" when the address is null or all parts are empty.
    /// </summary>
    public static string FormatAddress(AddressResponse? address)
    {
        if (address is null)
        {
            return "-";
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(address.Street)) parts.Add(address.Street);
        if (!string.IsNullOrWhiteSpace(address.Unit)) parts.Add(address.Unit);
        if (!string.IsNullOrWhiteSpace(address.City)) parts.Add(address.City);
        if (!string.IsNullOrWhiteSpace(address.State)) parts.Add(address.State);
        if (!string.IsNullOrWhiteSpace(address.PostalCode)) parts.Add(address.PostalCode);

        var combined = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(combined) ? "-" : combined;
    }
    public static ExtractedAddressComponents ExtractAddressComponents(List<AddressComponent> components)
    {
        return new ExtractedAddressComponents
        {
            StreetNumber = GetComponentValue(components, AddressComponentTypes.StreetNumber),
            Route = GetComponentValue(components, AddressComponentTypes.Route),
            Locality = GetComponentValue(components, AddressComponentTypes.Locality),
            AdminLevel2 = GetComponentValue(components, AddressComponentTypes.AdminLevel2),
            Province = GetComponentValue(components, AddressComponentTypes.AdminLevel1),
            PostalCode = GetComponentValue(components, AddressComponentTypes.PostalCode),
            Country = GetComponentValue(components, AddressComponentTypes.Country),
            CountryIso2 = GetComponentShortValue(components, AddressComponentTypes.Country)?.ToLowerInvariant() ?? string.Empty
        };
    }

    private static string GetComponentValue(List<AddressComponent> components, string type)
    {
        return components
            .Where(c => c.Types.Contains(type))
            .Select(c => c.LongName)
            .FirstOrDefault(value => !IsPlusCode(value))
            ?? string.Empty;
    }

    private static bool IsPlusCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();

        // Open/compound plus codes always include '+' and no spaces.
        return trimmed.Contains('+') && !trimmed.Contains(' ');
    }

    private static string GetComponentShortValue(List<AddressComponent> components, string type)
    {
        return components.FirstOrDefault(c => c.Types.Contains(type))?.ShortName ?? string.Empty;
    }

    public static string BuildAddressLine(ExtractedAddressComponents components)
    {
        if (!string.IsNullOrWhiteSpace(components.StreetNumber) && !string.IsNullOrWhiteSpace(components.Route))
        {
            return $"{components.StreetNumber} {components.Route}";
        }

        return new[]
            {
                components.Route,
                components.Locality,
                components.AdminLevel2,
                components.Province,
                components.Country
            }
         .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
         ?? string.Empty;
    }

    public static string BuildAddressLineFromGraphHopper(GraphHopperGeocodeHit hit)
    {
        if (!string.IsNullOrWhiteSpace(hit.HouseNumber) && !string.IsNullOrWhiteSpace(hit.Street))
        {
            return $"{hit.HouseNumber} {hit.Street}";
        }

        return new[]
            {
                hit.Street,
                hit.City,
                hit.State,
                hit.Country
            }
         .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
         ?? hit.Name ?? string.Empty;
    }

    public static bool TryParseCoordinate(string value, double min, double max, out double coordinate)
    {
        coordinate = 0;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value
            .Trim()
            .Replace(',', '.');

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out coordinate))
            return false;

        return double.IsFinite(coordinate) && coordinate >= min && coordinate <= max;
    }

    public static GeocodingCoordinateValidationResult Validate(string? latitude, string? longitude)
    {
        var result = new GeocodingCoordinateValidationResult();

        // Missing values
        var isLatMissing = string.IsNullOrWhiteSpace(latitude);
        var isLngMissing = string.IsNullOrWhiteSpace(longitude);

        if (isLatMissing || isLngMissing)
        {
            result.IsLatitudeMissing = isLatMissing;
            result.IsLongitudeMissing = isLngMissing;
            return result;
        }

        // Parsing + range validation
        var isLatValid = TryParseCoordinate(latitude!, -90, 90, out var lat);
        var isLngValid = TryParseCoordinate(longitude!, -180, 180, out var lng);

        if (!isLatValid || !isLngValid)
        {
            result.IsLatitudeInvalid = !isLatValid;
            result.IsLongitudeInvalid = !isLngValid;
            return result;
        }

        result.Latitude = lat;
        result.Longitude = lng;

        return result;
    }
}
