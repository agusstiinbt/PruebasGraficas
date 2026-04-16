namespace CigoWeb.Core.Helpers;

public static class GridifyFilterHelper
{
    public static string? BuildOrContainsFilter(string? search, params string[] fields)
    {
        if (string.IsNullOrWhiteSpace(search) || fields.Length == 0)
        {
            return null;
        }

        var value = GridifyTableHelper.EscapeValue(search.Trim());
        var clause = string.Join("|", fields.Select(field => $"({field}=*{value}/i)"));
        return fields.Length > 1 ? $"({clause})" : clause;
    }

    public static string? BuildBooleanEqualsFilter(string field, bool? value)
    {
        if (string.IsNullOrWhiteSpace(field) || value is null)
        {
            return null;
        }

        return $"({field}={(value.Value ? "true" : "false")})";
    }

    public static string? BuildEnumEqualsFilter<TEnum>(string field, params TEnum?[] values) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(field) || values == null || values.Length == 0 || values.All(v => v == null))
        {
            return null;
        }

        // Gridify parses enums by NAME, not numeric value
        var enumValues = values.Where(v => v.HasValue).Select(v => v!.Value.ToString());
        return $"({field}={string.Join("|", enumValues)})";
    }

    public static string? CombineWithAnd(params string?[] filters)
    {
        var activeFilters = filters
            .Where(filter => !string.IsNullOrWhiteSpace(filter))
            .ToArray();

        return activeFilters.Length == 0 ? null : string.Join(",", activeFilters);
    }
}
