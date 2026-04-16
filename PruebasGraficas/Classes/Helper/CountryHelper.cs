namespace CigoWeb.Core.Helpers;

/// <summary>
/// Helper class for converting between country names and Countries integer values
/// </summary>
public static class CountryHelper
{
    public static readonly IReadOnlyDictionary<int, string> CountryIso2ByCode = new Dictionary<int, string>
    {
        [(int)Countries.Canada] = "ca",
        [(int)Countries.UnitedStates] = "us",
        [(int)Countries.Mexico] = "mx"
    };

    private static readonly Lazy<CountryPhoneInfo[]> _cachedCountriesPhoneInfo = new(BuildCountriesPhoneInfo);
    private static readonly Lazy<CountryPhoneInfo[]> _cachedCountriesPhoneInfoByDialCodeLengthDescending = new(() =>
        _cachedCountriesPhoneInfo.Value
            .OrderByDescending(x => x.DialCode.Length)
            .ToArray());
    private static readonly Lazy<IReadOnlyDictionary<string, CountryPhoneInfo>> _cachedCountryPhoneInfoByIsoCode = new(() =>
        _cachedCountriesPhoneInfo.Value.ToDictionary(x => x.IsoCode, StringComparer.OrdinalIgnoreCase));
    private static readonly Lazy<IReadOnlyDictionary<string, CountryPhoneInfo[]>> _cachedCountryPhoneInfoByDialCode = new(() =>
        _cachedCountriesPhoneInfo.Value
            .GroupBy(x => x.DialCode, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToArray(), StringComparer.Ordinal));
    private static readonly Lazy<string[]> _cachedCountryNames = new(BuildCountryNames);

    /// <summary>
    /// Converts a country name string to its Countries integer value
    /// </summary>
    /// <param name="countryName">The country name (e.g., "Canada", "United States")</param>
    /// <returns>The integer enum value, or 0 (TBD) if not found</returns>
    public static int ParseCountryName(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            return 0; // TBD
        }

        // Normalize the country name for parsing
        var normalized = NormalizeCountryName(countryName);

        if (Enum.TryParse<Countries>(normalized, ignoreCase: true, out var result))
        {
            return (int)result;
        }

        return 0; // TBD if not found
    }

    /// <summary>
    /// Converts a Countries integer value to its display name
    /// </summary>
    /// <param name="countryCode">The integer country code (0-197)</param>
    /// <returns>The country name, or empty string if invalid</returns>
    public static string GetCountryName(int countryCode)
    {
        if (!Enum.IsDefined(typeof(Countries), countryCode))
        {
            return string.Empty;
        }

        var enumValue = (Countries)countryCode;
        var name = enumValue.ToString();

        // Format the enum name for display (e.g., "UnitedStates" -> "United States")
        return FormatCountryName(name);
    }

    /// <summary>
    /// Converts a Countries integer value to its display name, but hides TBD.
    /// </summary>
    /// <param name="countryCode">The integer country code (0-197)</param>
    /// <returns>The country name, or empty string for TBD/invalid values</returns>
    public static string GetCountryDisplayName(int countryCode)
    {
        return countryCode == (int)Countries.TBD
            ? string.Empty
            : GetCountryName(countryCode);
    }

    /// <summary>
    /// Normalizes a country name for enum parsing by removing spaces and special characters
    /// </summary>
    private static string NormalizeCountryName(string countryName)
    {
        return countryName
            .Replace(" ", "")
            .Replace("-", "")
            .Replace(".", "")
            .Replace("'", "")
            .Trim();
    }

    /// <summary>
    /// Formats an enum name for display by adding spaces before capital letters
    /// </summary>
    private static string FormatCountryName(string enumName)
    {
        if (string.IsNullOrWhiteSpace(enumName))
        {
            return string.Empty;
        }

        // Add space before each capital letter (except the first one)
        var result = new System.Text.StringBuilder();
        result.Append(enumName[0]);

        for (int i = 1; i < enumName.Length; i++)
        {
            if (char.IsUpper(enumName[i]))
            {
                result.Append(' ');
            }
            result.Append(enumName[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Returs a full list of information about countries on the CountryPhoneInfo class. Example "Argentina, AR, +54" 
    /// </summary>
    /// <returns></returns>
    public static List<CountryPhoneInfo> GetCountriesPhoneInfo()
    {
        return _cachedCountriesPhoneInfo.Value
            .Select(CreateCountryPhoneInfoCopy)
            .ToList();
    }

    /// <summary>
    /// Returns a list of country names (e.g., "Argentina", "United States", etc.)
    /// </summary>
    /// <returns></returns>
    public static List<string> GetCountryNames() => _cachedCountryNames.Value.ToList();

    internal static IReadOnlyList<CountryPhoneInfo> CachedCountriesPhoneInfoByDialCodeLengthDescending => _cachedCountriesPhoneInfoByDialCodeLengthDescending.Value;

    internal static bool TryGetCachedCountryPhoneInfoByIsoCode(string? isoCode, out CountryPhoneInfo? countryPhoneInfo)
    {
        countryPhoneInfo = null;
        if (string.IsNullOrWhiteSpace(isoCode))
        {
            return false;
        }

        return _cachedCountryPhoneInfoByIsoCode.Value.TryGetValue(isoCode, out countryPhoneInfo);
    }

    internal static IReadOnlyList<CountryPhoneInfo> GetCachedCountryPhoneInfoByDialCode(string? dialCode)
    {
        if (string.IsNullOrWhiteSpace(dialCode))
        {
            return Array.Empty<CountryPhoneInfo>();
        }

        return _cachedCountryPhoneInfoByDialCode.Value.TryGetValue(dialCode, out var countryPhoneInfo)
            ? countryPhoneInfo
            : Array.Empty<CountryPhoneInfo>();
    }

    internal static CountryPhoneInfo CreateCountryPhoneInfoCopy(CountryPhoneInfo countryPhoneInfo) => new()
    {
        IsoCode = countryPhoneInfo.IsoCode,
        DialCode = countryPhoneInfo.DialCode,
        CountryName = countryPhoneInfo.CountryName
    };

    public static string NormalizeCountryLookupKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant();
        var buffer = new char[normalized.Length];
        var index = 0;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[index++] = character;
            }
        }

        return new string(buffer, 0, index);
    }

    public static IReadOnlyDictionary<string, string> BuildRegionIsoMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);
                if (!string.IsNullOrWhiteSpace(region.TwoLetterISORegionName) && region.TwoLetterISORegionName.Length == 2)
                {
                    var iso2 = region.TwoLetterISORegionName.ToLowerInvariant();
                    TryAddRegionLookup(map, region.EnglishName, iso2);
                    TryAddRegionLookup(map, region.NativeName, iso2);
                    TryAddRegionLookup(map, region.DisplayName, iso2);
                }
            }
            catch
            {
                // Ignore invalid culture/region combinations.
            }
        }

        return map;
    }

    private static void TryAddRegionLookup(IDictionary<string, string> map, string countryName, string iso2)
    {
        var normalizedName = CountryHelper.NormalizeCountryLookupKey(countryName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return;
        }

        map.TryAdd(normalizedName, iso2);
    }

    private static CountryPhoneInfo[] BuildCountriesPhoneInfo()
    {
        var util = PhoneNumberUtil.GetInstance();
        var countries = new List<CountryPhoneInfo>();
        var regions = util.GetSupportedRegions();

        foreach (var regionCode in regions)
        {
            try
            {
                var region = new RegionInfo(regionCode);

                countries.Add(new CountryPhoneInfo
                {
                    IsoCode = regionCode,
                    DialCode = util.GetCountryCodeForRegion(regionCode).ToString(),
                    CountryName = region.EnglishName
                });
            }
            catch (ArgumentException)
            {
                continue;
            }
        }

        return countries
            .OrderBy(x => x.IsoCode)
            .ToArray();
    }

    private static string[] BuildCountryNames()
    {
        var util = PhoneNumberUtil.GetInstance();
        var countryNames = new List<string>();
        var regions = util.GetSupportedRegions();

        foreach (var regionCode in regions)
        {
            try
            {
                var region = new RegionInfo(regionCode);

                countryNames.Add(region.EnglishName);
            }
            catch (ArgumentException)
            {
                continue;
            }
        }

        return countryNames
            .OrderBy(x => x)
            .ToArray();
    }
}
