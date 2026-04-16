using System.Net;
using MudBlazor;

namespace CigoWeb.Core.Helpers;

public static class CrudPageHelper
{
    public static DialogOptions DefaultDialogOptions(MaxWidth maxWidth = MaxWidth.Medium)
        => new()
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = maxWidth,
        };

    public static async Task<bool> OpenDialogAndReloadAsync<TDialog>(
        IDialogService dialogService,
        string title,
        DialogParameters? parameters,
        DialogOptions? options,
        ISnackbar snackbar,
        string successMessage,
        Action reload)
        where TDialog : IComponent
    {
        var dialog = parameters is null
            ? await dialogService.ShowAsync<TDialog>(title, options ?? DefaultDialogOptions())
            : await dialogService.ShowAsync<TDialog>(title, parameters, options ?? DefaultDialogOptions());

        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            snackbar.Add(successMessage, MudBlazor.Severity.Success);
            reload();
            return true;
        }

        return false;
    }

    public static async Task DeleteEntityAsync<TEntity>(
        IDialogService dialogService,
        ISnackbar snackbar,
        IStringLocalizer<LocalResources> localizer,
        string entityDisplayName,
        TEntity entity,
        Func<TEntity, string?> idSelector,
        Func<TEntity, string?> labelSelector,
        Func<string, Task<HttpStatusCode>> deleteAsync,
        Action reload)
    {
        var id = idSelector(entity);
        if (string.IsNullOrWhiteSpace(id))
        {
            snackbar.Add(localizer["Crud.DeleteMissingIdentifier", entityDisplayName], MudBlazor.Severity.Error);
            return;
        }

        var label = labelSelector(entity) ?? id;
        var safeLabel = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(label) ? "-" : label);
        if (!await dialogService.ConfirmDeleteAsync(
            title: localizer["Crud.ConfirmDeleteTitle", entityDisplayName],
            markupMessage: new MarkupString(localizer["Crud.ConfirmDeleteMessageHtml", safeLabel].Value),
            yesText: localizer["Crud.Delete"],
            cancelText: localizer["Cancel"]))
        {
            return;
        }

        try
        {
            var status = await deleteAsync(id);

            if (status is HttpStatusCode.OK or HttpStatusCode.NoContent)
            {
                snackbar.Add(localizer["Crud.DeletedSuccessfully"], MudBlazor.Severity.Success);
                reload();
            }
            else
            {
                snackbar.Add(localizer["Crud.DeleteFailedStatus", (int)status], MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add(localizer["Crud.DeleteUnable", entityDisplayName], MudBlazor.Severity.Error);
            Console.Error.WriteLine(ex);
        }
    }
}
