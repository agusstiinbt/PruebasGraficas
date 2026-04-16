namespace CigoWeb.Core.Helpers.AdvancedFilters;

/// <summary>
/// Describes how a dropdown should react after a parameter update.
/// </summary>
/// <param name="ShouldRequestClose">Indicates whether the component should notify its parent to close the dropdown.</param>
/// <param name="ShouldClearSearch">Indicates whether the current search text should be cleared.</param>
public readonly record struct DropdownStateTransition(bool ShouldRequestClose, bool ShouldClearSearch);

/// <summary>
/// Tracks dropdown open-state transitions across parameter updates.
/// It is intended for controlled advanced-filter dropdowns that need to detect
/// transitions such as open-to-closed or open-to-disabled without embedding that
/// state machine directly in the UI component.
/// </summary>
public sealed class DropdownStateTracker
{
    private bool _wasOpen;

    /// <summary>
    /// Updates the tracker with the latest dropdown state and calculates the transition effects.
    /// </summary>
    /// <param name="isOpen">Whether the dropdown is currently open according to the latest parameters.</param>
    /// <param name="isDisabled">Whether the dropdown is currently disabled.</param>
    /// <returns>
    /// A <see cref="DropdownStateTransition"/> describing whether the caller should request a close
    /// and whether any in-progress search state should be cleared.
    /// </returns>
    public DropdownStateTransition Update(bool isOpen, bool isDisabled)
    {
        var shouldRequestClose = isDisabled && isOpen;
        var effectiveIsOpen = shouldRequestClose ? false : isOpen;
        var shouldClearSearch = _wasOpen && !effectiveIsOpen;

        _wasOpen = effectiveIsOpen;

        return new DropdownStateTransition(shouldRequestClose, shouldClearSearch);
    }

    /// <summary>
    /// Synchronizes the tracker when the dropdown changes state internally rather than through parameter updates.
    /// </summary>
    /// <param name="isOpen">The new internal open state.</param>
    public void SetOpen(bool isOpen)
    {
        _wasOpen = isOpen;
    }
}