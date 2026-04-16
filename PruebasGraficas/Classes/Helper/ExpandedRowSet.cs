namespace CigoWeb.Core.Helpers;

public sealed class ExpandedRowSet<TId> where TId : notnull
{
    private readonly HashSet<TId> _expanded = new();

    public bool IsExpanded(TId id) => _expanded.Contains(id);

    public void Toggle(TId id)
    {
        if (!_expanded.Add(id))
        {
            _expanded.Remove(id);
        }
    }
}
