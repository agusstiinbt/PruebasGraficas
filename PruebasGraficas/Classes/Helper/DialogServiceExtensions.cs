using System.Net;
using MudBlazor;

namespace CigoWeb.Core.Helpers;

public static class DialogServiceExtensions
{
    public static async Task<bool> ConfirmDeleteAsync(this IDialogService dialogService, string entityName, string label)
    {
        var safeLabel = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(label) ? "-" : label);
        var confirmed = await dialogService.ConfirmDeleteAsync(
            title: $"Delete {entityName}",
            markupMessage: (MarkupString)$"Are you sure you want to delete <b>{safeLabel}</b>?",
            yesText: "Delete",
            cancelText: "Cancel");

        return confirmed is true;
    }

    public static async Task<bool> ConfirmDeleteAsync(
        this IDialogService dialogService,
        string title,
        MarkupString markupMessage,
        string yesText,
        string cancelText)
    {
        var confirmed = await dialogService.ShowMessageBoxAsync(
            title: title,
            markupMessage: markupMessage,
            yesText: yesText,
            cancelText: cancelText);

        return confirmed is true;
    }
}
