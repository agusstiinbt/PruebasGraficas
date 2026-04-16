namespace CigoWeb.Core.Helpers;

public static class EmailHelper
{
    public static string? GetValidateEmailMessage(string? email, IStringLocalizer<LocalResources> localizer)
    {
        if (string.IsNullOrWhiteSpace(email))
            return localizer["Validation.InvalidEmail"];

        if (email.Contains('"') || email.Contains(' '))
            return localizer["Validation.InvalidEmail"];

        var parts = email.Split('@');
        if (parts.Length != 2)
            return localizer["Validation.InvalidEmail"];

        var local = parts[0];
        var domain = parts[1];

        if (string.IsNullOrWhiteSpace(local))
            return localizer["Validation.InvalidEmail"];

        if (domain.StartsWith('.') || domain.EndsWith('.') || domain.Contains(".."))
            return localizer["Validation.InvalidEmail"];

        var domainParts = domain.Split('.');

        // example.com
        if (domainParts.Length == 2)
        {
            if (string.IsNullOrWhiteSpace(domainParts[0]) || string.IsNullOrWhiteSpace(domainParts[1]))
                return localizer["Validation.InvalidEmail"];
        }
        // example.com.ar
        else if (domainParts.Length == 3)
        {
            if (!string.Equals(domainParts[1], "com", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(domainParts[2], "ar", StringComparison.OrdinalIgnoreCase))
                return localizer["Validation.InvalidEmail"];
        }
        else
        {
            return localizer["Validation.InvalidEmail"];
        }

        return null;
    }
}
