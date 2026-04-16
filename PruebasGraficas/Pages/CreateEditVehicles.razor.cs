using MudBlazor;
using PruebasGraficas.Classes.Validator.Vehicle;

namespace PruebasGraficas.Pages;

public partial class CreateEditVehicles
{
    [Parameter] public int Id { get; set; }

    #region Injections

    [Inject] private IValidator<VehicleCreateEditModel> _validationRules { get; set; } = default!;
    [Inject] private NavigationManager _nav { get; set; } = default!;

    #endregion


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
    /// Tracks whether the user has explicitly chosen a departure time.
    /// Used to style the default value as a gray placeholder.
    /// </summary>
    private bool _departureTimeTouched { get; set; }

    private async Task OnDepartureTimeChanged(TimeOnly value)
    {
        _departureTimeTouched = true;
        _model.DepartureTime = value;
    }

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
        var cancellationToken = _cancellationToken?.Token ?? CancellationToken.None;

       
        _loading = false;
    }

    private void Cancel() => _nav.NavigateTo(IndexPage, true);


    #endregion

}