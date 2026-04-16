namespace CigoWeb.Core.Helpers.Address;

public record ExtractedAddressComponents
{
    public string StreetNumber { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Locality { get; init; } = string.Empty;
    public string AdminLevel2 { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string CountryIso2 { get; init; } = string.Empty;
}
