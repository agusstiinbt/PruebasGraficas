namespace CigoWeb.Core.Helpers;

public static class TagCollectionHelper
{
    public static List<string> Sanitize(IEnumerable<string>? tags)
        => tags?
            .Where(static tag => !string.IsNullOrWhiteSpace(tag))
            .Select(static tag => tag.Trim())
            .ToList()
        ?? [];

    public static int CountSanitized(IEnumerable<string>? tags) => Sanitize(tags).Count;
}
