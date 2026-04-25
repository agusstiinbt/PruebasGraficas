using MudBlazor;
using PruebasGraficas.Classes.Validator.Vehicle;

namespace PruebasGraficas.Pages;

public partial class CreateEditVehicles
{
    [Parameter] public int Id { get; set; }

    #region Injections

    [Inject] private IValidator<VehicleCreateEditModel> _validationRules { get; set; } = default!;
    [Inject] private NavigationManager _nav { get; set; } = default!;
    [Inject] private ISnackbar _snack { get; set; } = default!;

    #endregion

    public int value { get; set; }
    private MudForm _form = new();
    public required VehicleCreateEditModel _model = new();

    private CancellationTokenSource? _cancellationToken { get; set; }

    private readonly string IndexPage = "/vehicles";
    private string? _error { get; set; }


    /// <summary>
    /// This works to show loading on the front end as well as blocking buttons
    /// </summary>
    private bool _loading { get; set; }

    /// <summary>
    /// Bool for MudSwitch at the top
    /// </summary>
    private bool _deactivate
    {
        get => !_model.IsActive;
        set => _model.IsActive = !value;
    }

    protected override async Task OnInitializedAsync()
    {
        _cancellationToken = new CancellationTokenSource();

        var token = _cancellationToken.Token;

        _loading = true;


        _loading = false;
    }

    #region Submit & Cancel behaviour

    private async Task Submit()
    {
        _error = null;
        _loading = true;

        await _form.ValidateAsync();

        if (_form.IsValid)
            await ProcessSubmit();

        StateHasChanged();
        _loading = false;
    }

    private async Task ProcessSubmit()
    {
        _snack.Add("Success", MudBlazor.Severity.Success);
        var cancellationToken = _cancellationToken?.Token ?? CancellationToken.None;

        _loading = false;
    }

    private void Cancel() => _nav.NavigateTo(IndexPage, true);

    #endregion

}