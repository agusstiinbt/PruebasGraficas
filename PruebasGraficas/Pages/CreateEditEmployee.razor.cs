using MudBlazor;
using PruebasGraficas.Classes.Models.Location;
using PruebasGraficas.Classes.Passwords;
using PruebasGraficas.Classes.Validator.Employee;

namespace PruebasGraficas.Pages
{
    public partial class CreateEditEmployee : IDisposable
    {
        //This file follows documentation of MudBlazor: https://mudblazor.com/components/form#using-fluent-validation
        [Parameter] public int Id { get; set; }

        #region Injections

        [Inject] private ISnackbar _snackBar { get; set; } = default!;
        //[Inject] private IMapper _mapper { get; set; } = default!;
        //[Inject] private ICountryCodeService _countryCodeService { get; set; } = default!;
        //[Inject] private IEmployeesService _employeesService { get; set; } = default!;
        //[Inject] private ILocationsService _locationsService { get; set; } = default!;
        [Inject] private IValidator<EmployeeCreateEditModel> _validationRules { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        #endregion

        public List<CountryPhoneInfo> CountriesPhoneInfo { get; set; } = CountryHelper.GetCountriesPhoneInfo();

        private List<LocationResponse> _locations { get; set; } = new();

        //private ManagedFileUpload? _logoUpload { get; set; }
        private MudForm _form { get; set; } = new();
        private CancellationTokenSource? _cancellationToken { get; set; }
        public EmployeeCreateEditModel _model { get; set; } = new();


        /// <summary>
        /// /web-users
        /// </summary>
        private readonly string IndexPage = "/web-users";
        private string? _error { get; set; }
        private string _textValue = string.Empty;
        private string _stringValue = string.Empty;

        private int _selectedCountryCode;

        private double? _latitude;
        private double? _longitude;


        /// <summary>
        /// Bool for MudSwitch at the top
        /// </summary>
        private bool _deactivate
        {
            get => !_model.IsActive;
            set => _model.IsActive = !value;
        }

        /// <summary>
        /// This works to show loading on the front end as well as blocking buttons
        /// </summary>
        private bool _loading { get; set; }


        protected override async Task OnInitializedAsync()
        {
            _cancellationToken = new CancellationTokenSource();
            var cancellationToken = _cancellationToken.Token;

            _loading = true;

            //var locationsResult = await _locationsService.GetAllAsync(cancellationToken: cancellationToken);
            //if (locationsResult.Any())
            //    _locations = locationsResult!
            //        .Where(location => location.IsActive)
            //        .ToList();

            if (Id != 0)
            {
                //var response = await _employeesService.GetByIdAsync(Id, cancellationToken);

                //if (response == null)
                //{
                //    _loading = false;
                //    _snackBar.Add(_localizer["CreateEditEmployee_Employee_NotFound"], MudBlazor.Severity.Error);
                //    _nav.NavigateTo(IndexPage, true); // Force reload maybe employee still in cache?
                //    return;
                //}

                //_model = _mapper.Map<EmployeeCreateEditModel>(response);

                //_latitude = response.Address?.Latitude;
                //_longitude = response.Address?.Longitude;
            }

            _selectedCountryCode = CountryHelper.ParseCountryName(_model.Country);

            _loading = false;

            _passwordStrength = new(false, 0, Color.Error, string.Empty);
        }

        public void Dispose()
        {
            _cancellationToken?.Cancel();
            _cancellationToken?.Dispose();
        }

        #region Password

        private PasswordVisibility _passwordVis { get; set; } = new(false, InputType.Password, Icons.Material.Filled.VisibilityOff);

        private PasswordVisibility _confirmPasswordVis { get; set; } = new(false, InputType.Password, Icons.Material.Filled.VisibilityOff);

        private PasswordStrengthResult _passwordStrength { get; set; } = new(false, 0, Color.Error, string.Empty);

        private async Task OnPasswordChanged()
        {
            _passwordStrength = PasswordStrengthHelper.Calculate(_model.Password, _localizer);
            if (!string.IsNullOrEmpty(_model.ConfirmPassword))
            {
                await _form.ValidateAsync();
            }
        }

        private void TogglePasswordVisibility(bool confirmPassword)
        {
            if (confirmPassword)
                _confirmPasswordVis = Toggle(_confirmPasswordVis);
            else
                _passwordVis = Toggle(_passwordVis);

            static PasswordVisibility Toggle(PasswordVisibility current) => current.IsVisible
                ? new(false, InputType.Password, Icons.Material.Filled.VisibilityOff)
                : new(true, InputType.Text, Icons.Material.Filled.Visibility);
        }


        #endregion

        #region OnChanged Events

        private void OnCountryChanged(int value)
        {
            if (_selectedCountryCode == value)
                return;

            _selectedCountryCode = value;
            _model.Country = CountryHelper.GetCountryName(value);
        }

        private void OnLatitudeChanged(double? value)
        {
            _latitude = value;

            var normalized = value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            if (_model.Latitude == normalized)
                return;

            _model.Latitude = normalized;
        }

        private void OnLongitudeChanged(double? value)
        {
            _longitude = value;

            var normalized = value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            if (_model.Longitude == normalized)
                return;

            _model.Longitude = normalized;
        }

        private void OnAllowManualAddressEntryChanged(bool value)
        {

        }

        #endregion

        private string GetLocationDisplayName(int? locationId)
        {
            if (locationId is null)
            {
                return string.Empty;
            }

            var locationName = _locations.FirstOrDefault(location => location.Id == locationId.Value)?.Name;
            return string.IsNullOrWhiteSpace(locationName)
                ? locationId.Value.ToString(CultureInfo.InvariantCulture)
                : locationName;
        }

        #region Submit & Cancel behaviour

        private void Cancel() => _nav.NavigateTo(IndexPage);

        private async Task Submit()
        {
            _error = null;
            _loading = true;

            await _form.ValidateAsync();

            if (_form.IsValid)
                await ProcessSubmit();

            _loading = false;
        }

        private async Task ProcessSubmit()
        {
            var cancellationToken = _cancellationToken?.Token ?? CancellationToken.None;
            //var employeeRequest = _mapper.Map<EmployeeRequest>(_model);
        }

        private string ExtractErrorMessage(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                return message;

            return _localizer["Crud.TryAgainLater"];
        }

        private string BuildLocalizedErrorMessage(string resourceKey, string entityLabel, string detail)
        {
            var template = _localizer[resourceKey].Value;
            var baseMessage = template.Contains("{0}", StringComparison.Ordinal)
                ? string.Format(CultureInfo.CurrentCulture, template, entityLabel)
                : string.Concat(template, " ", entityLabel);

            return string.Concat(baseMessage, ": ", detail);
        }

        #endregion
    }
}
