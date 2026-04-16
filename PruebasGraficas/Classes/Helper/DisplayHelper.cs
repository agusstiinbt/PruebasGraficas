using System.Text;
using TimeZoneConverter;

namespace CigoWeb.Core.Helpers;

public static class DisplayHelper
{
    public static decimal? NormalizeNonNegative(decimal? value, int decimalPlaces = 2)
    {
        if (!value.HasValue)
            return null;

        var normalizedValue = value.Value < 0m ? 0m : value.Value;
        var scaleFactor = (decimal)Math.Pow(10, decimalPlaces);
        return Math.Truncate(normalizedValue * scaleFactor) / scaleFactor;
    }

    public static string? FormatDateTime(DateTime? dateTime)
        => !dateTime.HasValue || dateTime.Value == default ? null : dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public static string DisplayOrDash(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    public static string DisplayOrDash(int? value) => DisplayOrDash(value?.ToString());

    public static string DisplayOrDash(DateTime? value) => DisplayOrDash(FormatDateTime(value));

    public static string FormatTimeZoneDisplay(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return "—";
        }

        try
        {
            var tz = TZConvert.GetTimeZoneInfo(timeZoneId);
            var offset = tz.GetUtcOffset(DateTimeOffset.UtcNow);
            var sign = offset >= TimeSpan.Zero ? "+" : "-";
            var abs = offset.Duration();
            var hours = (int)abs.TotalHours;
            var minutes = abs.Minutes;
            return $"(UTC{sign}{hours:00}:{minutes:00}) {timeZoneId}";
        }
        catch
        {
            return timeZoneId;
        }
    }

    /// <summary>
    /// Returns the full name by combining first and last name, 
    /// handling cases where either may be missing or whitespace. 
    /// </summary>
    /// <param name="FirstName">The first name of the person.</param>
    /// <param name="LastName">The last name of the person.</param>
    /// <returns>The full name, or the available name if one is missing.</returns>
    public static string GetFullName(string FirstName, string LastName)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            return LastName;

        if (string.IsNullOrWhiteSpace(LastName))
            return FirstName;

        var builder = new StringBuilder(FirstName.Length + 1 + LastName.Length);
        builder.Append(FirstName);
        builder.Append(' ');
        builder.Append(LastName);

        return builder.ToString();
    }

    public static string GetUserInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 1)
        {
            return parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant();
        }

        var first = parts[0].Length > 0 ? parts[0][0].ToString() : string.Empty;
        var last = parts[^1].Length > 0 ? parts[^1][0].ToString() : string.Empty;
        var initials = (first + last).ToUpperInvariant();

        return string.IsNullOrWhiteSpace(initials) ? "?" : initials;
    }
}
