namespace PruebasGraficas.Pages.Shared.Form

public partial class ManagedFileUpload
{
    [Inject] private IFilesService FilesService { get; set; } = default!;
    [Inject] private ILogger<ManagedFileUpload> Logger { get; set; } = default!;
    [Inject] private IStringLocalizer<LocalResources> Localizer { get; set; } = default!;

    [Parameter] public int? EmployeeId { get; set; }
    [Parameter] public int? LocationId { get; set; }
    [Parameter] public int? OperatorId { get; set; }
    [Parameter] public FileType FileType { get; set; } = FileType.Logo;
    [Parameter] public Languages Language { get; set; } = Languages.English;
    [Parameter] public bool IsPublic { get; set; } = true;
    [Parameter] public string? DescriptionForSave { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Description { get; set; }
    [Parameter] public string? HelperText { get; set; }
    [Parameter] public string? SecondaryHelperText { get; set; }
    [Parameter] public string? FooterText { get; set; }
    [Parameter] public string? ButtonText { get; set; }
    [Parameter] public bool EnableCropModal { get; set; } = true;
    [Parameter] public string CropAspectRatio { get; set; } = "1:1";
    [Parameter] public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024;
    [Parameter] public List<string>? AcceptedFileTypes { get; set; }
    [Parameter] public string Height { get; set; } = "400px";

    [Parameter] public EventCallback<FileUploadOperationResult> OnOperationCompleted { get; set; }

    private FileResponse? _existingFile { get; set; }
    private string? _existingFileUrl { get; set; }
    private string? _existingFileName { get; set; }
    private byte[] _pendingBytes { get; set; } = Array.Empty<byte>();
    private string? _pendingFileName { get; set; }
    private bool _pendingDeletion { get; set; }
    private string _cacheToken { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    private int? _lastEmployeeId { get; set; }
    private int? _lastLocationId { get; set; }
    private int? _lastOperatorId { get; set; }

    private string EffectiveDescriptionForSave => string.IsNullOrWhiteSpace(DescriptionForSave)
        ? Localizer["ImageUpload.DefaultDescriptionForSave"]
        : DescriptionForSave;
    private string DefaultFileNameWithExtension => string.Concat(Localizer["ImageUpload.DefaultFileName"], ".png");
    private string OwnerRequiredMessage => Localizer["ImageUpload.Error.OwnerRequired"];
    private string GenericUploadErrorMessage => Localizer["Crud.TryAgainLater"];

    protected override async Task OnParametersSetAsync()
    {
        if (_lastEmployeeId == EmployeeId && _lastLocationId == LocationId && _lastOperatorId == OperatorId)
            return;

        _lastEmployeeId = EmployeeId;
        _lastLocationId = LocationId;
        _lastOperatorId = OperatorId;

        if (!TryResolveOwner(EmployeeId, LocationId, OperatorId, out var employeeId, out var locationId, out var operatorId))
        {
            ResetPreload();
            return;
        }

        if (employeeId is null && locationId is null && operatorId is null)
        {
            ResetPreload();
            return;
        }

        await LoadExistingFileAsync(employeeId, locationId, operatorId);
    }

    public Task SetPendingSelectionAsync(ImageUploadResult? result) => OnFileChangedAsync(result);

    public async Task<FileUploadOperationResult> SavePendingAsync(int? employeeId = null, int? locationId = null, int? operatorId = null)
    {
        var deleteFailure = await DeleteExistingFileIfRequestedAsync();
        if (deleteFailure is not null)
        {
            return deleteFailure;
        }

        if (_pendingBytes.Length == 0)
        {
            var noPending = new FileUploadOperationResult
            {
                Status = FileUploadOperationStatus.NoPendingFile,
                File = _existingFile,
            };

            await OnOperationCompleted.InvokeAsync(noPending);
            return noPending;
        }

        if (!TryResolveOwner(employeeId ?? EmployeeId, locationId ?? LocationId, operatorId ?? OperatorId, out var resolvedEmployeeId, out var resolvedLocationId, out var resolvedOperatorId))
        {
            var invalidOwner = new FileUploadOperationResult
            {
                Status = FileUploadOperationStatus.Failed,
                ErrorMessage = OwnerRequiredMessage,
            };

            await OnOperationCompleted.InvokeAsync(invalidOwner);
            return invalidOwner;
        }

        if (resolvedEmployeeId is null && resolvedLocationId is null && resolvedOperatorId is null)
        {
            var missingOwner = new FileUploadOperationResult
            {
                Status = FileUploadOperationStatus.Failed,
                ErrorMessage = OwnerRequiredMessage,
            };

            await OnOperationCompleted.InvokeAsync(missingOwner);
            return missingOwner;
        }

        try
        {
            var fileRequest = new FileRequest
            {
                Name = string.IsNullOrWhiteSpace(_pendingFileName) ? DefaultFileNameWithExtension : _pendingFileName,
                Description = EffectiveDescriptionForSave,
                EmployeeId = resolvedEmployeeId,
                LocationId = resolvedLocationId,
                OperatorId = resolvedOperatorId,
                Language = Language,
                FileType = FileType,
                Stream = _pendingBytes,
                IsPublic = IsPublic,
                IsActive = true,
            };

            if (_existingFile?.Id is > 0)
            {
                fileRequest.Id = _existingFile.Id;
                fileRequest.RowVersion = _existingFile.RowVersion;
            }

            var saved = await FilesService.SaveAsync(fileRequest);

            _existingFile = saved;
            _existingFileName = saved.Name;
            _existingFileUrl = saved.Url is { Length: > 0 } url
                ? UrlHelper.AppendCacheBustingToken(url)
                : null;

            _pendingBytes = Array.Empty<byte>();
            _pendingFileName = null;

            var success = new FileUploadOperationResult
            {
                Status = FileUploadOperationStatus.Succeeded,
                File = saved,
            };

            await OnOperationCompleted.InvokeAsync(success);
            return success;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "File upload failed for employee {EmployeeId}, location {LocationId}, operator {OperatorId}.", resolvedEmployeeId, resolvedLocationId, resolvedOperatorId);

            var failed = new FileUploadOperationResult
            {
                Status = FileUploadOperationStatus.Failed,
                ErrorMessage = GenericUploadErrorMessage,
            };

            await OnOperationCompleted.InvokeAsync(failed);
            return failed;
        }
    }

    private async Task OnFileChangedAsync(ImageUploadResult? result)
    {
        if (result is null)
        {
            _pendingBytes = Array.Empty<byte>();
            _pendingFileName = null;
            _pendingDeletion = _existingFile?.Id is > 0;
            return;
        }

        _pendingBytes = result.Bytes;
        _pendingFileName = result.FileName;
        _pendingDeletion = false;
        await Task.CompletedTask;
    }

    private async Task<FileUploadOperationResult?> DeleteExistingFileIfRequestedAsync()
    {
        if (!_pendingDeletion)
        {
            return null;
        }

        if (_existingFile?.Id is not > 0)
        {
            _pendingDeletion = false;
            return null;
        }

        try
        {
            _ = await FilesService.DeleteAsync(_existingFile.Id.ToString(CultureInfo.InvariantCulture));

            ResetPreload();
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to delete existing file {FileId}.", _existingFile.Id);

            var failed = new FileUploadOperationResult
            {
                Status = FileUploadOperationStatus.Failed,
                ErrorMessage = GenericUploadErrorMessage,
            };

            await OnOperationCompleted.InvokeAsync(failed);
            return failed;
        }
    }

    private async Task LoadExistingFileAsync(int? employeeId, int? locationId, int? operatorId)
    {
        try
        {
            var ownerFilter = employeeId is > 0
                ? $"(EmployeeId={employeeId})"
                : locationId is > 0
                    ? $"(LocationId={locationId})"
                    : operatorId is > 0
                        ? $"(OperatorId={operatorId})"
                        : null;

            if (string.IsNullOrWhiteSpace(ownerFilter))
            {
                ResetPreload();
                return;
            }

            var filter = GridifyFilterHelper.CombineWithAnd(
                ownerFilter,
                GridifyFilterHelper.BuildEnumEqualsFilter<FileType>(nameof(FileRequest.FileType), FileType));

            var paging = await FilesService.GetPagingResultsAsync(
                pageNumber: 1,
                pageSize: 1,
                orderBy: "Id desc",
                filter: filter);

            _existingFile = paging?.Data?.FirstOrDefault();
            _existingFileName = _existingFile?.Name;
            _existingFileUrl = _existingFile?.Url is { Length: > 0 } url
                ? UrlHelper.AppendCacheBustingToken(url, _cacheToken)
                : null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to preload file for employee {EmployeeId}, location {LocationId}, operator {OperatorId}.", employeeId, locationId, operatorId);
            ResetPreload();
        }
    }

    private static bool TryResolveOwner(int? employeeId, int? locationId, int? operatorId, out int? resolvedEmployeeId, out int? resolvedLocationId, out int? resolvedOperatorId)
    {
        resolvedEmployeeId = employeeId is > 0 ? employeeId : null;
        resolvedLocationId = locationId is > 0 ? locationId : null;
        resolvedOperatorId = operatorId is > 0 ? operatorId : null;

        // Exactly one owner type must be set
        var ownerCount = (resolvedEmployeeId is not null ? 1 : 0)
            + (resolvedLocationId is not null ? 1 : 0)
            + (resolvedOperatorId is not null ? 1 : 0);

        if (ownerCount > 1)
            return false;

        return true;
    }

    private void ResetPreload()
    {
        _existingFile = null;
        _existingFileUrl = null;
        _existingFileName = null;
        _pendingDeletion = false;
    }

}
