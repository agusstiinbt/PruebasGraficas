namespace CigoWeb.Core.Helpers;

public static class UrlHelper
{
    public static string AppendCacheBustingToken(string url)
        => AppendCacheBustingToken(url, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));

    public static string AppendCacheBustingToken(string url, string token)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(token))
        {
            return url;
        }

        var separator = url.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{url}{separator}v={Uri.EscapeDataString(token)}";
    }
}
