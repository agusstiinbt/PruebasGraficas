namespace CigoWeb.Core.Helpers;

public static partial class PhoneNumberHelper
{
    public static (string DialCode, string PhoneNumber) ExtractPhoneParts(string? fullPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullPhoneNumber))
        {
            return (string.Empty, string.Empty);
        }

        var match = CountryHelper.CachedCountriesPhoneInfoByDialCodeLengthDescending
            .FirstOrDefault(x => fullPhoneNumber.StartsWith(x.DialCode, StringComparison.Ordinal));

        if (match is null)
        {
            return (string.Empty, fullPhoneNumber);
        }

        var dialCode = match.DialCode;
        var phoneNumber = fullPhoneNumber.Substring(dialCode.Length);

        return (dialCode, phoneNumber);
    }

    /// <summary>
    /// Resolves the <see cref="CountryPhoneInfo"/> that best matches the given full phone number,
    /// handling ambiguous dial codes (e.g. +1 shared by US/CA) via libphonenumber region inference.
    /// </summary>
    public static CountryPhoneInfo? ResolveCountryPhoneInfo(string? phoneNumber, string? isoCode)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        // ISO always has more priority
        if (CountryHelper.TryGetCachedCountryPhoneInfoByIsoCode(isoCode, out var matchByIso))
        {
            return CountryHelper.CreateCountryPhoneInfoCopy(matchByIso!);
        }

        var (dialCode, _) = ExtractPhoneParts(phoneNumber);

        if (string.IsNullOrWhiteSpace(dialCode))
            return null;

        //DialCode but no ISO, we check ambiguity
        var sameDial = CountryHelper.GetCachedCountryPhoneInfoByDialCode(dialCode);

        // If there is only one there is no ambiguity and we return it
        if (sameDial.Count == 1)
            return CountryHelper.CreateCountryPhoneInfoCopy(sameDial[0]);

        try
        {
            var util = PhoneNumberUtil.GetInstance();
            var parsed = util.Parse(phoneNumber, null);
            var region = util.GetRegionCodeForNumber(parsed); //ISO alpha-2

            if (!string.IsNullOrWhiteSpace(region))
            {
                var regionMatch = sameDial.FirstOrDefault(c =>
                    string.Equals(c.IsoCode, region, StringComparison.OrdinalIgnoreCase));

                if (regionMatch != null)
                    return CountryHelper.CreateCountryPhoneInfoCopy(regionMatch);
            }
        }
        catch (NumberParseException)
        {
            //fallback
        }

        return sameDial.Count > 0
            ? CountryHelper.CreateCountryPhoneInfoCopy(sameDial[0])
            : null;
    }

    /// <summary>
    /// Returns a string with the error of the given number or an empty string if the number is valid.
    /// Uses CountryPhoneInfo.IsoCode as the region code for accurate validation,
    /// especially for ambiguous dial codes like +1 (US/CA).
    /// </summary>
    /// <param name="countryPhoneInfo">The selected country phone info containing dial code and ISO code.</param>
    /// <param name="phoneNumber">The phone number without the dial code prefix.</param>
    /// <param name="localizer">String localizer for error messages.</param>
    /// <returns>An error message string, or empty if the number is valid.</returns>
    public static string GetValidationMessage(CountryPhoneInfo? countryPhoneInfo, string? phoneNumber, IStringLocalizer<LocalResources> localizer)
    {
        if (countryPhoneInfo is null || string.IsNullOrWhiteSpace(countryPhoneInfo.DialCode))
            return localizer["CountryCode_Is_Necessary"];

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return localizer["PhoneNumber_Is_Necessary"];

        if (!PhoneNumberRegex().IsMatch(phoneNumber))
            return localizer["PhoneNumber_Invalid_Characters"];

        var util = PhoneNumberUtil.GetInstance();

        // Prefer ISO code for region-accurate validation (fixes ambiguous +1 US/CA)
        var regionCode = countryPhoneInfo.IsoCode;
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            var numericDialCode = ParseDialCode(countryPhoneInfo.DialCode);
            if (numericDialCode <= 0)
                return localizer["Country_DialCode_Is_Invalid"];

            regionCode = util.GetRegionCodeForCountryCode(numericDialCode);
        }

        if (string.IsNullOrWhiteSpace(regionCode))
            return localizer["Country_DialCode_Is_Invalid"];

        try
        {
            var parsedNumber = util.Parse(phoneNumber.Trim(), regionCode);
            var possibleResult = util.IsPossibleNumberWithReason(parsedNumber);

            switch (possibleResult)
            {
                case PhoneNumberUtil.ValidationResult.TOO_SHORT:
                    return localizer["PhoneNumber_Too_Short_For_Selected_Country"];
                case PhoneNumberUtil.ValidationResult.TOO_LONG:
                    return localizer["PhoneNumber_Too_Long_For_Selected_Country"];
                case PhoneNumberUtil.ValidationResult.INVALID_LENGTH:
                    return localizer["PhoneNumber_Length_Invalid_For_Selected_Country"];
                case PhoneNumberUtil.ValidationResult.INVALID_COUNTRY_CODE:
                    return localizer["Invalid_Country_Calling_Code"];
                case PhoneNumberUtil.ValidationResult.IS_POSSIBLE_LOCAL_ONLY:
                    return localizer["PhoneNumber_Appears_To_Be_Missing_Area_Or_Mobile_Code"];
                case PhoneNumberUtil.ValidationResult.IS_POSSIBLE:
                    return string.Empty;
            }

            if (!util.IsValidNumberForRegion(parsedNumber, regionCode))
                return localizer["PhoneNumber_Length_Invalid_For_Selected_Country"];

            //If empty then it means the number is valid
            return string.Empty;
        }
        catch (NumberParseException ex)
        {
            return ex.ErrorType switch
            {
                ErrorType.NOT_A_NUMBER => (string)localizer["PhoneNumber_Invalid_Format"],
                ErrorType.TOO_SHORT_NSN => (string)localizer["PhoneNumber_Is_Too_Short"],
                ErrorType.TOO_LONG => (string)localizer["PhoneNumber_Is_Too_Long"],
                ErrorType.INVALID_COUNTRY_CODE => (string)localizer["CountryCode_Invalid"],
                _ => (string)localizer["PhoneNumber_Invalid"],
            };
        }
    }

    private static int ParseDialCode(string dialCode)
    {
        var cleaned = dialCode.TrimStart('+');
        return int.TryParse(cleaned, out var result) ? result : 0;
    }

    [GeneratedRegex(@"^[0-9+\-\(\)\s]+$")]
    private static partial Regex PhoneNumberRegex();
}
