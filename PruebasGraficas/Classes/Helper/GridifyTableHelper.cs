using MudBlazor;

namespace CigoWeb.Core.Helpers;

public static class GridifyTableHelper
{
    public static string? BuildOrderBy(TableState state)
    {
        if (string.IsNullOrWhiteSpace(state.SortLabel) || state.SortDirection == SortDirection.None)
        {
            return null;
        }

        return state.SortDirection == SortDirection.Descending
            ? $"{state.SortLabel} desc"
            : state.SortLabel;
    }

    public static string EscapeValue(string raw)
    {
        // Escape Gridify reserved characters with a backslash.
        // Do NOT wrap in double quotes — spaces and other characters are handled by URL-encoding
        // in QueryHelpers.AddQueryString; quoted values cause parse failures in the backend.
        return raw
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("%", "\\%") // Gridify wildcard/operator (e.g., contains); escape to treat as a literal value
            .Replace("$", "\\$") // Gridify wildcard/operator (e.g., starts/ends-with); escape to treat as a literal value
            .Replace("^", "\\^") // Gridify operator modifier; escape to prevent it from altering the filter semantics
            .Replace("<", "\\<") // Gridify comparison operator; escape when used as a literal character
            .Replace(">", "\\>") // Gridify comparison operator; escape when used as a literal character
            .Replace("=", "\\=") // Gridify equality operator; escape when used as a literal character
            .Replace(",", "\\,")
            .Replace("|", "\\|")
            .Replace("&", "\\&")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }
}
