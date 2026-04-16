namespace CigoWeb.Core.Helpers.AdvancedFilters;

/// <summary>
/// Tracks at most one open item at a time for a group of related UI elements.
/// Opening a new item implicitly closes any previously open item, which makes it
/// suitable for scenarios such as mutually exclusive dropdowns identified by enum values.
/// </summary>
/// <typeparam name="TId">The identifier type for each trackable item, typically an enum or stable key.</typeparam>
public sealed class SingleOpenState<TId> where TId : notnull
{
    private bool _hasOpenItem;
    private TId? _openItem;

    /// <summary>
    /// Determines whether the specified item is currently the open item.
    /// </summary>
    /// <param name="id">The identifier of the item to inspect.</param>
    /// <returns><c>true</c> when the specified item is open; otherwise, <c>false</c>.</returns>
    public bool IsOpen(TId id)
        => _hasOpenItem && EqualityComparer<TId>.Default.Equals(_openItem!, id);

    /// <summary>
    /// Opens or closes the specified item.
    /// Opening an item makes it the only open item in the group, replacing any previously open item.
    /// Closing an item only clears the state when that item is currently open.
    /// </summary>
    /// <param name="id">The identifier of the item to update.</param>
    /// <param name="isOpen"><c>true</c> to mark the item as open; <c>false</c> to close it.</param>
    public void SetOpen(TId id, bool isOpen)
    {
        if (isOpen)
        {
            _openItem = id;
            _hasOpenItem = true;
            return;
        }

        if (IsOpen(id))
        {
            Clear();
        }
    }

    /// <summary>
    /// Clears the current open item so that no item remains open.
    /// </summary>
    public void Clear()
    {
        _openItem = default;
        _hasOpenItem = false;
    }
}