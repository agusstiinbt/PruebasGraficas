using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using PruebasGraficas.Classes.Models;

namespace PruebasGraficas.Pages.Shared.Form;
/// <summary>
/// Reusable image upload component with optional crop modal.
/// Emits <see cref="ImageUploadResult"/> via <see cref="OnFileChanged"/>.
/// </summary>
public partial class ImageUploadInput : ComponentBase, IAsyncDisposable
{
    // ── Parameters ──────────────────────────────────────────────────────────

    /// <summary>URL of an existing image to preload into the picker on first render.</summary>
    [Parameter] public string? PreloadUrl { get; set; }

    /// <summary>Optional display name for the preloaded image.</summary>
    [Parameter] public string? PreloadFileName { get; set; }

    /// <summary>Fires when the user has selected (and optionally cropped) an image, or null when removed.</summary>
    [Parameter] public EventCallback<ImageUploadResult?> OnFileChanged { get; set; }

    /// <summary>When true (default), a MudDialog crop modal appears after file selection.</summary>
    [Parameter] public bool EnableCropModal { get; set; } = true;

    /// <summary>Aspect ratio string used by the crop modal, e.g. "1:1", "16:9". Default is "1:1".</summary>
    [Parameter] public string CropAspectRatio { get; set; } = "1:1";

    /// <summary>Maximum allowed file size in bytes. Default is 2 MB.</summary>
    [Parameter] public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024;

    /// <summary>MIME types accepted by the picker. Defaults to PNG and JPEG.</summary>
    [Parameter] public List<string>? AcceptedFileTypes { get; set; }

    /// <summary>CSS height of the outer dashed box. Default is "400px".</summary>
    [Parameter] public string Height { get; set; } = "400px";

    /// <summary>Optional subtitle label rendered above the box.</summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>Descriptive text shown inside the upload area.</summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>Secondary helper text shown under the description.</summary>
    [Parameter] public string? HelperText { get; set; }

    /// <summary>Optional secondary helper text rendered below <see cref="HelperText"/>.</summary>
    [Parameter] public string? SecondaryHelperText { get; set; }

    /// <summary>Optional explanatory text rendered below the upload box.</summary>
    [Parameter] public string? FooterText { get; set; }

    /// <summary>Choose-file button caption.</summary>
    [Parameter] public string? ButtonText { get; set; }

    // ── Injected services ───────────────────────────────────────────────────

    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<ImageUploadInput> Logger { get; set; } = default!;
    [Inject] private IStringLocalizer<LocalResources> Localizer { get; set; } = default!;

    // ── Private state ───────────────────────────────────────────────────────

    private string? _validationError { get; set; }
    private string? _previewDataUrl { get; set; }
    private string? _currentFileName { get; set; }
    private string? _lastPreloadSignature { get; set; }
    private string _wrapperId { get; } = $"img-upload-{Guid.NewGuid():N}";
    private string _inputId { get; } = $"img-upload-input-{Guid.NewGuid():N}";
    private ElementReference _dropZoneRef { get; set; }
    private DotNetObjectReference<ImageUploadInput>? _dotNetRef { get; set; }
    private long _dropZoneRegistrationId { get; set; }
    private bool _isDragOver { get; set; }
    private int _processingFlag;
    private const string DefaultWrapperClass = "image-upload-wrapper border border-dashed";
    private bool HasPreview => !string.IsNullOrWhiteSpace(_previewDataUrl);
    private bool IsProcessing => Volatile.Read(ref _processingFlag) == 1;
    private string WrapperClass => _isDragOver ? $"{DefaultWrapperClass} mud-border-primary" : DefaultWrapperClass;
    private string EffectiveDescription => string.IsNullOrWhiteSpace(Description)
        ? Localizer["ImageUpload.DefaultDescription"]
        : Description;
    private string? EffectiveHelperText => string.IsNullOrWhiteSpace(HelperText)
        ? Localizer["ImageUpload.DefaultHelperText"]
        : HelperText;
    private string? EffectiveSecondaryHelperText => string.IsNullOrWhiteSpace(SecondaryHelperText)
        ? null
        : SecondaryHelperText;
    private string EffectiveButtonText => string.IsNullOrWhiteSpace(ButtonText)
        ? Localizer["ImageUpload.ButtonText"]
        : ButtonText;

    private static readonly string[] _defaultAcceptedExtensions = [".png", ".jpg", ".jpeg"];

    // ── Lifecycle ───────────────────────────────────────────────────────────

    protected override void OnInitialized()
    {
        ApplyPreloadFromParameters();
    }

    protected override void OnParametersSet()
    {
        ApplyPreloadFromParameters();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _dotNetRef = DotNetObjectReference.Create(this);
        _dropZoneRegistrationId = await JSRuntime.InvokeAsync<long>(
            "imageUploadInterop.registerDropZone",
            _dropZoneRef,
            _dotNetRef);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dropZoneRegistrationId > 0)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("imageUploadInterop.unregisterDropZone", _dropZoneRegistrationId);
            }
            catch
            {
                // Ignore teardown JS errors during dispose.
            }
        }

        _dotNetRef?.Dispose();
    }

    // ── Event handlers ──────────────────────────────────────────────────────

    private async Task OpenFilePickerAsync()
    {
        if (IsProcessing)
            return;

        await JSRuntime.InvokeVoidAsync("imageUploadInterop.openFilePickerById", _inputId);
    }

    private async Task RemoveImageAsync()
    {
        if (IsProcessing)
            return;

        _validationError = null;
        _previewDataUrl = null;
        _currentFileName = null;
        await InvokeAsync(StateHasChanged);
        await OnFileChanged.InvokeAsync(null);
    }

    private async Task OnInputFileSelectedAsync(InputFileChangeEventArgs args)
    {
        if (!TryBeginProcessing())
            return;

        try
        {
            var file = args.File;
            if (file is null)
                return;

            if (file.Size > MaxFileSizeBytes)
            {
                _validationError = string.Format(
                    CultureInfo.CurrentCulture,
                    Localizer["ImageUpload.Error.SizeExceeded"],
                    MaxFileSizeBytes / (1024 * 1024));
                await InvokeAsync(StateHasChanged);
                return;
            }

            byte[]? bytes = null;
            try
            {
                await using var stream = file.OpenReadStream(MaxFileSizeBytes);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            catch (Exception ex)
            {
                _validationError = Localizer["ImageUpload.Error.ReadSelectedFile"];
                Logger.LogError(ex, "Error reading file from InputFile.");
                await InvokeAsync(StateHasChanged);
                return;
            }

            await ProcessSelectedFileAsync(file.Name, file.ContentType, bytes);
        }
        finally
        {
            EndProcessing();
        }
    }

    [JSInvokable]
    public async Task OnDragStateChanged(bool isDragOver)
    {
        _isDragOver = isDragOver;
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnDropFileAsync(string fileName, string contentType, string dataUrl)
    {
        if (!TryBeginProcessing())
            return;

        try
        {
            if (string.IsNullOrWhiteSpace(dataUrl))
                return;

            var commaIndex = dataUrl.IndexOf(',', StringComparison.Ordinal);
            if (commaIndex < 0 || commaIndex + 1 >= dataUrl.Length)
                return;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(dataUrl[(commaIndex + 1)..]);
            }
            catch (FormatException)
            {
                _validationError = Localizer["ImageUpload.Error.ReadDroppedFile"];
                await InvokeAsync(StateHasChanged);
                return;
            }

            await ProcessSelectedFileAsync(fileName, contentType, bytes);
        }
        finally
        {
            EndProcessing();
        }
    }

    private async Task ProcessSelectedFileAsync(string? fileName, string? contentType, byte[]? rawBytes)
    {
    }

    private bool TryBeginProcessing()
    {
        if (Interlocked.CompareExchange(ref _processingFlag, 1, 0) != 0)
            return false;

        _ = InvokeAsync(StateHasChanged);
        return true;
    }

    private void EndProcessing()
    {
        Interlocked.Exchange(ref _processingFlag, 0);
        _ = InvokeAsync(StateHasChanged);
    }

    private static string DeriveContentType(string ext) => ext switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream",
    };

    private static decimal ParseAspectRatio(string ratio)
    {
        if (string.IsNullOrWhiteSpace(ratio))
            return 1m;

        var parts = ratio.Split(':');
        if (parts.Length == 2
            && decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var num)
            && decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var den)
            && den != 0)
            return num / den;

        if (decimal.TryParse(ratio, NumberStyles.Number, CultureInfo.InvariantCulture, out var direct))
            return direct;

        return 1m;
    }

    private void ApplyPreloadFromParameters()
    {
        var signature = $"{PreloadUrl}|{PreloadFileName}";
        if (string.Equals(signature, _lastPreloadSignature, StringComparison.Ordinal))
            return;

        _lastPreloadSignature = signature;

        if (string.IsNullOrWhiteSpace(PreloadUrl))
        {
            _previewDataUrl = null;
            _currentFileName = null;
            return;
        }

        _previewDataUrl = PreloadUrl;
        _currentFileName = ResolvePreloadFileName();
    }

    private string ResolvePreloadFileName()
    {
        if (!string.IsNullOrWhiteSpace(PreloadFileName))
            return PreloadFileName;

        if (Uri.TryCreate(PreloadUrl, UriKind.Absolute, out var absoluteUri))
        {
            var absoluteName = Path.GetFileName(absoluteUri.AbsolutePath);
            if (!string.IsNullOrWhiteSpace(absoluteName))
                return absoluteName;
        }

        if (Uri.TryCreate(PreloadUrl, UriKind.Relative, out var relativeUri))
        {
            var relativeName = Path.GetFileName(relativeUri.OriginalString);
            if (!string.IsNullOrWhiteSpace(relativeName))
                return relativeName;
        }

        return Localizer["ImageUpload.CurrentImageFallback"];
    }
}

