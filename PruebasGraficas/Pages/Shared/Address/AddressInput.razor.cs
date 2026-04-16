namespace PruebasGraficas.Pages.Shared.Address
{
    public partial class AddressInput : IAsyncDisposable
    {
        [Inject] private ILogger<AddressInput> Logger { get; set; } = default!;
        //[Inject] private IAddressService _addressService { get; set; } = default!;
        [Inject] private IJSRuntime _JS { get; set; } = default!;

        private DotNetObjectReference<AddressInput>? _dotNetRef;
        private static readonly Lazy<IReadOnlyDictionary<string, string>> _regionISO2ByCountryName = new(CountryHelper.BuildRegionIsoMap);

        #region Parameters

        private List<CountryPhoneInfo> CountriesPhoneInfo { get; set; } = CountryHelper.GetCountriesPhoneInfo();

        [Parameter]
        public int SearchDebounceMilliseconds { get; set; } = 120;

        [Parameter]
        public string Address { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<string> AddressChanged { get; set; }

        [Parameter]
        public string? Unit { get; set; }

        [Parameter]
        public EventCallback<string?> UnitChanged { get; set; }

        [Parameter]
        public string? PostalCode { get; set; }

        [Parameter]
        public EventCallback<string?> PostalCodeChanged { get; set; }

        [Parameter]
        public string? Province { get; set; }

        [Parameter]
        public EventCallback<string?> ProvinceChanged { get; set; }

        [Parameter]
        public int Country { get; set; }

        [Parameter]
        public EventCallback<int> CountryChanged { get; set; }

        [Parameter]
        public double? Latitude { get; set; }

        [Parameter]
        public EventCallback<double?> LatitudeChanged { get; set; }

        [Parameter]
        public double? Longitude { get; set; }

        [Parameter]
        public EventCallback<double?> LongitudeChanged { get; set; }

        [Parameter]
        public bool Required { get; set; } = false;

        [Parameter]
        public bool AllowManualAddressEntry { get; set; } = false;

        [Parameter]
        public EventCallback<bool> AllowManualAddressEntryChanged { get; set; }

        [Parameter]
        public bool ShowAddressLabel { get; set; } = true;

        #endregion

        private int _requestVersion { get; set; } = 0;

        private const int MinSearchCharacters = 3;

        #region bools
        private bool _jsInitialized { get; set; } = false;
        private bool UseAutoAddressSuggestions => !_allowManualAddressEntry;
        private bool _keepSuggestionsOpen { get; set; }
        private bool _showAddressSuggestions { get; set; }
        private bool _hasTriggeredSuggestionSearch { get; set; }
        private bool _apiError { get; set; }
        private bool _useCoordinates { get; set; }
        private bool _selectedFromSuggestions { get; set; }
        private bool _allowManualAddressEntry { get; set; }
        private bool _showAutoSuggestionToggle { get; set; }
        private bool HasValidationError => !string.IsNullOrWhiteSpace(_validationError);
        private bool IsAddressRequiredError => string.Equals(
            _validationError,
            _localizer["AddressInput_Error_AddressRequired"].Value,
            StringComparison.Ordinal);
        private bool ShouldHighlightAddressShell => HasValidationError && !IsAddressRequiredError;
        private string AddressLabelClass => IsAddressRequiredError ? "field-label has-error" : "field-label";
        private string AddressShellClass => ShouldHighlightAddressShell ? "address-input-shell has-error" : "address-input-shell";
        private string AddressInputClass => IsAddressRequiredError ? "address-input-text has-error" : "address-input-text";

        #endregion

        #region strings
        private string AddressHelperText => HasValidationError
            ? _validationError!
            : _allowManualAddressEntry
                ? string.Empty
                : _localizer["AddressInput_AddressHelper_Default"];

        public string F6 { get; set; } = "F6";
        private string _currentAddress { get; set; } = string.Empty;
        private string _validationError { get; set; } = string.Empty;
        private string _extractedAddress { get; set; } = string.Empty;
        private string _latitudeValidationError { get; set; } = string.Empty;
        private string _longitudeValidationError { get; set; } = string.Empty;
        private string _unitValidationError { get; set; } = string.Empty;
        private string _postalCodeValidationError { get; set; } = string.Empty;
        private string _stateValidationError { get; set; } = string.Empty;
        private string _countryValidationError { get; set; } = string.Empty;
        private string _latitude { get; set; } = string.Empty;
        private string _longitude { get; set; } = string.Empty;
        private string _currentCountry { get; set; } = string.Empty;
        private string _currentCountryIso2 { get; set; } = string.Empty;
        private string AddressInputElementId { get; set; } = $"address-input-{Guid.NewGuid():N}";
        private string _currentProvince { get; set; } = string.Empty;
        private string _currentUnit { get; set; } = string.Empty;
        private string _currentPostalCode { get; set; } = string.Empty;
        #endregion

        private IReadOnlyList<string> _countryOptions { get; set; } = CountryHelper.GetCountryNames();
        private IReadOnlyList<PlacePrediction> _addressSuggestions { get; set; } = Array.Empty<PlacePrediction>();

        private int _activeSuggestionIndex { get; set; } = -1;

        private PlacePrediction? _selectedPrediction { get; set; }
        private CancellationTokenSource? _addressSearchCancellationTokenSource { get; set; }

        #region LifeCycle 

        protected override void OnParametersSet()
        {
            var province = Province ?? string.Empty;
            var unit = Unit ?? string.Empty;
            var postalCode = PostalCode ?? string.Empty;

            if (_allowManualAddressEntry != AllowManualAddressEntry)
            {
                _allowManualAddressEntry = AllowManualAddressEntry;

                if (_allowManualAddressEntry)
                {
                    _hasTriggeredSuggestionSearch = false;
                    CloseAddressSuggestions();
                }
            }

            if (_currentAddress != Address)
                _currentAddress = Address;

            // In edit mode we may receive a persisted address that was already validated/saved.
            // Seed it once as the extracted baseline so unchanged values do not fail suggestion-only validation.
            if (!string.IsNullOrWhiteSpace(Address))
            {
                _extractedAddress = Address;
                _selectedFromSuggestions = true;
            }

            if (_currentProvince != province)
                _currentProvince = province;

            if (_currentUnit != unit)
                _currentUnit = unit;

            if (_currentPostalCode != postalCode)
                _currentPostalCode = postalCode;

            if (!_allowManualAddressEntry || string.IsNullOrWhiteSpace(_currentCountry))
            {
                var countryName = CountryHelper.GetCountryDisplayName(Country);

                if (_currentCountry != countryName)
                    _currentCountry = countryName;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            _allowManualAddressEntry = AllowManualAddressEntry;

            if (Latitude.HasValue)
            {
                _latitude = Latitude.Value.ToString(F6, CultureInfo.InvariantCulture);
            }
            if (Longitude.HasValue)
            {
                _longitude = Longitude.Value.ToString(F6, CultureInfo.InvariantCulture);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!_jsInitialized)
            {
                try
                {
                    _dotNetRef ??= DotNetObjectReference.Create(this);

                    await _JS.InvokeVoidAsync(
                        "addressInterop.registerInput",
                        _dotNetRef,
                        AddressInputElementId);

                    _jsInitialized = true;
                }
                catch (InvalidOperationException)
                {
                    // pre-render ignore
                }
            }
        }

        #endregion

        private string GetCountryFlagCssClass()
        {
            var iso2 = GetCountryFlagIso2();
            return string.IsNullOrWhiteSpace(iso2) ? string.Empty : $"iti__{iso2}";
        }

        private string GetCountryFlagIso2()
        {
            if (!string.IsNullOrWhiteSpace(_currentCountryIso2))
            {
                return _currentCountryIso2;
            }

            var countryName = _allowManualAddressEntry
                ? _currentCountry
                : CountryHelper.GetCountryName(Country);

            var normalizedCountryName = CountryHelper.NormalizeCountryLookupKey(countryName);


            if (!string.IsNullOrWhiteSpace(normalizedCountryName) && normalizedCountryName != "tbd")
            {
                var isoCountry = CountriesPhoneInfo
                    .Where(x => !string.IsNullOrWhiteSpace(x.CountryName))
                    .Select(x => new
                    {
                        x.IsoCode,
                        NormalizedCountryName = CountryHelper.NormalizeCountryLookupKey(x.CountryName)
                    })
                    .FirstOrDefault(x => x.NormalizedCountryName == normalizedCountryName)
                    ?.IsoCode;

                if (!string.IsNullOrWhiteSpace(isoCountry))
                {
                    return isoCountry.ToLowerInvariant();
                }

                if (_regionISO2ByCountryName.Value.TryGetValue(normalizedCountryName, out var mappedIso2))
                {
                    return mappedIso2;
                }
            }

            var countryCode = _allowManualAddressEntry
                ? CountryHelper.ParseCountryName(_currentCountry)
                : Country;

            return CountryHelper.CountryIso2ByCode.TryGetValue(countryCode, out var iso2)
                ? iso2
                : string.Empty;
        }

        private void CloseAddressSuggestions()
        {
            _addressSuggestions = Array.Empty<PlacePrediction>();
            _showAddressSuggestions = false;
            _activeSuggestionIndex = -1;
        }

        private void ToggleCoordinatesAndSuggestionMode()
        {
            _useCoordinates = !_useCoordinates;
            _validationError = string.Empty;

            ClearLatLngValidationError();

            if (_useCoordinates)
            {
                if (Latitude.HasValue)
                {
                    _latitude = Latitude.Value.ToString(F6);
                }
                if (Longitude.HasValue)
                {
                    _longitude = Longitude.Value.ToString(F6);
                }
            }
            else
            {
                ClearLatLngValues();
            }

            _showAutoSuggestionToggle = _useCoordinates;
        }

        private Task<IEnumerable<string>> SearchCountries(string value, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.FromResult(_countryOptions.AsEnumerable());
            }

            var countries = _countryOptions
                .Where(country => country.Contains(value, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(countries);
        }

        /// <summary>
        /// Cleans validation error of latitude and longitude
        /// </summary>
        private void ClearLatLngValidationError()
        {
            _latitudeValidationError = string.Empty;
            _longitudeValidationError = string.Empty;
        }

        /// <summary>
        /// Cleans values of latitude and longitude
        /// </summary>
        private void ClearLatLngValues()
        {
            _latitude = string.Empty;
            _longitude = string.Empty;
        }

        private async Task TriggerReverseGeocoding()
        {
            ClearLatLngValidationError();

            var validation = AddressHelper.Validate(_latitude, _longitude);

            if (!validation.IsValid)
            {
                if (validation.IsLatitudeMissing)
                    _latitudeValidationError = _localizer["Latitude_Required"].Value;
                else if (validation.IsLatitudeInvalid)
                    _latitudeValidationError = _localizer["AddressInput_Error_InvalidLatitudeRange"].Value;

                if (validation.IsLongitudeMissing)
                    _longitudeValidationError = _localizer["Longitude_Required"].Value;
                else if (validation.IsLongitudeInvalid)
                    _longitudeValidationError = _localizer["AddressInput_Error_InvalidLongitudeRange"].Value;

                return;
            }

            var lat = validation.Latitude!.Value;
            var lng = validation.Longitude!.Value;

            _validationError = string.Empty;

            var success = await GetGeographicInfo(lat, lng);

            if (!success)
                await GetGeographicPosition(lat, lng);

            await Task.WhenAll(
                LatitudeChanged.InvokeAsync(lat),
                LongitudeChanged.InvokeAsync(lng)
            );
        }

        private async Task GetGeographicPosition(double lat, double lng)
        {
            _addressSearchCancellationTokenSource?.Cancel();
            _addressSearchCancellationTokenSource?.Dispose();

            var cts = new CancellationTokenSource();
            _addressSearchCancellationTokenSource = cts;

            try
            {
                //var request = new GraphHopperGeocodeRequest
                //{
                //    Reverse = true,
                //    PointLat = lat,
                //    PointLon = lng,
                //    Limit = 1
                //};

                //var (success, hit, addressLine, countryCode, province) = await _addressService.GetGraphGeoCodeResponse(request, cts.Token);

                //if (success)
                //{
                //    _apiError = false;
                //    _currentPostalCode = hit.Postcode ?? string.Empty;
                //    _currentAddress = addressLine;
                //    _extractedAddress = addressLine;
                //    _currentProvince = province!;
                //    _currentCountry = hit.Country ?? string.Empty;
                //    // Added to remove error label when populated by suggestion
                //    _stateValidationError = string.Empty;
                //    _countryValidationError = string.Empty;
                //    _selectedFromSuggestions = true;

                //    await Task.WhenAll(
                //        AddressChanged.InvokeAsync(addressLine),
                //        PostalCodeChanged.InvokeAsync(_currentPostalCode),
                //        CountryChanged.InvokeAsync(countryCode),
                //        ProvinceChanged.InvokeAsync(province),
                //        LatitudeChanged.InvokeAsync(lat),
                //        LongitudeChanged.InvokeAsync(lng)
                //    );

                //    StateHasChanged();
                //    return;
                //}

            }
            catch (Exception)
            {
                _validationError = _localizer["AddressInput_Error_RetrieveDetails"].Value;
            }
        }

        private async Task<bool> GetGeographicInfo(double lat, double lng)
        {
            //try
            //{
            //    var response = await _addressService.GetGeoCoding(lat, lng);

            //    if (response != null && response.Status == "OK" && response.Results != null && response.Results.Count > 0)
            //    {
            //        _apiError = false;
            //        var result = SelectBestGeocodingResult(response.Results);

            //        if (result is null)
            //            return false;

            //        var components = result.AddressComponents;
            //        var addressComponents = AddressHelper.ExtractAddressComponents(components ?? new List<AddressComponent>());
            //        var addressLine = AddressHelper.BuildAddressLine(addressComponents);
            //        var countryCode = CountryHelper.ParseCountryName(addressComponents.Country);
            //        var postalCode = addressComponents.PostalCode ?? string.Empty;

            //        _currentAddress = addressLine;
            //        _extractedAddress = addressLine;
            //        _currentProvince = addressComponents.Province;
            //        _currentPostalCode = postalCode;
            //        _currentCountry = addressComponents.Country;
            //        _currentCountryIso2 = addressComponents.CountryIso2;
            //        _stateValidationError = string.Empty;
            //        _countryValidationError = string.Empty;
            //        _selectedFromSuggestions = true;

            //        await Task.WhenAll(
            //            AddressChanged.InvokeAsync(addressLine),
            //            PostalCodeChanged.InvokeAsync(_currentPostalCode),
            //            CountryChanged.InvokeAsync(countryCode),
            //            ProvinceChanged.InvokeAsync(addressComponents.Province),
            //            LatitudeChanged.InvokeAsync(lat),
            //            LongitudeChanged.InvokeAsync(lng)
            //        );

            //        StateHasChanged();
            //        return true;
            //    }
            //    return false;
            //}
            //catch (Exception)
            //{
            //    _validationError = _localizer["AddressInput_Error_RetrieveDetails"].Value;
            //}
            return false;
        }

        private static GeocodingResult? SelectBestGeocodingResult(IEnumerable<GeocodingResult> results)
        {
            //foreach (var result in results)
            //{
            //    if (IsPlusCodeOnlyResult(result.AddressComponents))
            //        continue;

            //    var components = AddressHelper.ExtractAddressComponents(result.AddressComponents ?? new List<AddressComponent>());
            //    var addressLine = AddressHelper.BuildAddressLine(components);

            //    if (!string.IsNullOrWhiteSpace(addressLine))
            //        return result;
            //}

            return null;
        }

        //private static bool IsPlusCodeOnlyResult(List<AddressComponent>? components)
        //{
        //    if (components == null || components.Count == 0)
        //        return false;

        //    var type = "plus_code";

        //    foreach (var component in components)
        //    {
        //        return string.Equals(component.Types, type);
        //    }
        //    return !HasAddressBearingType(components);
        //}

        //private static bool HasAddressBearingType(IEnumerable<AddressComponent> components)
        //{
        //    foreach (var component in components)
        //    {
        //        if (component.Types == null)
        //            continue;

        //        foreach (var type in component.Types)
        //        {
        //            if (string.Equals(type, AddressComponentTypes.StreetNumber, StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(type, AddressComponentTypes.Route, StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(type, AddressComponentTypes.Locality, StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(type, AddressComponentTypes.AdminLevel1, StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(type, AddressComponentTypes.AdminLevel2, StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(type, AddressComponentTypes.PostalCode, StringComparison.OrdinalIgnoreCase)
        //                || string.Equals(type, AddressComponentTypes.Country, StringComparison.OrdinalIgnoreCase))
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        #region Events

        private void OnSuggestionMouseDown() => _keepSuggestionsOpen = true;

        private async Task OnAddressSuggestionClicked(PlacePrediction prediction)
        {
            //if (prediction == null)
            //{
            //    return;
            //}

            //_selectedPrediction = prediction;
            //_selectedFromSuggestions = true;

            //try
            //{
            //    var (success, placeDetailsResponse, components, addressComponents, addressLine, countryCode) = await _addressService.GetPlaceDetails(prediction);

            //    if (!success)
            //    {
            //        Logger.LogWarning(
            //            "Google Place Details API error: {Status} - {ErrorMessage}",
            //            placeDetailsResponse?.Status,
            //            placeDetailsResponse?.ErrorMessage);
            //        _apiError = true;
            //        _validationError = _localizer["AddressInput_Error_LookupUnavailable"].Value;
            //        return;
            //    }

            //    if (addressComponents == null)
            //        return;

            //    _apiError = false;
            //    _validationError = string.Empty;
            //    _currentPostalCode = addressComponents.PostalCode ?? string.Empty;
            //    _currentAddress = addressLine!;
            //    _extractedAddress = addressLine!;
            //    _currentProvince = addressComponents.Province;
            //    _currentCountry = addressComponents.Country;
            //    _currentCountryIso2 = addressComponents.CountryIso2;
            //    _stateValidationError = string.Empty;
            //    _countryValidationError = string.Empty;

            //    double? lat = placeDetailsResponse!.Result!.Geometry!.Location!.Lat;
            //    double? lng = placeDetailsResponse.Result.Geometry.Location?.Lng;

            //    _latitude = lat?.ToString(F6) ?? string.Empty;
            //    _longitude = lng?.ToString(F6) ?? string.Empty;

            //    await Task.WhenAll(
            //        AddressChanged.InvokeAsync(addressLine),
            //        PostalCodeChanged.InvokeAsync(_currentPostalCode),
            //        CountryChanged.InvokeAsync(countryCode!.Value),
            //        ProvinceChanged.InvokeAsync(addressComponents.Province),
            //        LatitudeChanged.InvokeAsync(lat),
            //        LongitudeChanged.InvokeAsync(lng)
            //    );

            //    StateHasChanged();
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogWarning(ex, "Failed to retrieve place details");
            //    _validationError = _localizer["AddressInput_Error_RetrieveDetails"].Value;
            //    _apiError = true;
            //}

            //CloseAddressSuggestions();
        }

        private Task OnAutoSuggestionChanged(bool useAutoAddressSuggestions)
        {
            _allowManualAddressEntry = !useAutoAddressSuggestions;

            if (_allowManualAddressEntry)
            {
                _hasTriggeredSuggestionSearch = false;
                CloseAddressSuggestions();
            }

            return AllowManualAddressEntryChanged.InvokeAsync(_allowManualAddressEntry);
        }

        [JSInvokable]
        public Task OnClickOutside()
        {
            CloseAddressSuggestions();
            StateHasChanged();

            return Task.CompletedTask;
        }

        [JSInvokable]
        public async Task OnAddressInputFromJs(string text)
        {
            _currentAddress = text;

            await OnAddressChangedInternal(text);
        }

        private async Task OnAddressChangedInternal(string text)
        {
            _currentAddress = text;

            if (_allowManualAddressEntry
                && !string.IsNullOrWhiteSpace(text)
                && (_validationError == _localizer["AddressInput_Error_AddressRequired"].Value
                    || _validationError == _localizer["AddressInput_Error_SelectFromSuggestions"].Value))
            {
                _validationError = string.Empty;
            }

            // versionated to avoid race conditions. The latest is the one we search for
            int currentVersion = ++_requestVersion;

            bool matchesPrediction = _selectedPrediction != null && _selectedPrediction.Description == text;
            bool matchesExtracted = !string.IsNullOrWhiteSpace(_extractedAddress) && _extractedAddress == text;

            if (!matchesPrediction && !matchesExtracted)
            {
                _selectedFromSuggestions = false;
                _extractedAddress = string.Empty;

                if (!string.IsNullOrWhiteSpace(_latitude) || !string.IsNullOrWhiteSpace(_longitude) || Latitude.HasValue || Longitude.HasValue)
                {
                    ClearLatLngValues();

                    await Task.WhenAll(
                        LatitudeChanged.InvokeAsync(null),
                        LongitudeChanged.InvokeAsync(null)
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                ClearLatLngValues();

                await Task.WhenAll(
                    LatitudeChanged.InvokeAsync(null),
                    LongitudeChanged.InvokeAsync(null)
                );
            }

            await AddressChanged.InvokeAsync(text);

            await UpdateAddressSuggestionsAsync(text, currentVersion);
        }

        private async Task UpdateAddressSuggestionsAsync(string text, int version)
        {
            _addressSearchCancellationTokenSource?.Cancel();
            _addressSearchCancellationTokenSource?.Dispose();

            var cts = new CancellationTokenSource();
            _addressSearchCancellationTokenSource = cts;

            var token = cts.Token;

            if (_allowManualAddressEntry || string.IsNullOrWhiteSpace(text) || text.Length < MinSearchCharacters)
            {
                CloseAddressSuggestions();
                return;
            }

            try
            {
                await Task.Delay(SearchDebounceMilliseconds, token);

                //var response = await _addressService.SearchAddresses(text, _allowManualAddressEntry, token);

                if (token.IsCancellationRequested || version != _requestVersion)
                    return;

                //var results = response.ToList();
                //_apiError = false;

                //_addressSuggestions = results;
                //_showAddressSuggestions = results.Count > 0;
                //_activeSuggestionIndex = results.Count > 0 ? 0 : -1;

                StateHasChanged();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task OnUnitChanged(string? unit)
        {
            _currentUnit = unit ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_unitValidationError))
            {
                _unitValidationError = string.Empty;
            }
            await UnitChanged.InvokeAsync(_currentUnit);
        }

        private async Task OnPostalCodeChanged(string? postalCode)
        {
            _currentPostalCode = postalCode ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_postalCodeValidationError))
            {
                _postalCodeValidationError = string.Empty;
            }
            await PostalCodeChanged.InvokeAsync(_currentPostalCode);
        }

        private async Task OnProvinceChanged(string? province)
        {
            var value = province ?? string.Empty;

            Province = value;
            _currentProvince = value;
            if (!string.IsNullOrWhiteSpace(_stateValidationError))
            {
                _stateValidationError = string.Empty;
            }

            await ProvinceChanged.InvokeAsync(value);
        }

        private async Task OnCountryChanged(string? countryName)
        {
            var previousCountry = _currentCountry;
            _currentCountry = countryName ?? string.Empty;
            _currentCountryIso2 = string.Empty;
            if (!string.IsNullOrWhiteSpace(_countryValidationError))
            {
                _countryValidationError = string.Empty;
            }
            await CountryChanged.InvokeAsync(CountryHelper.ParseCountryName(_currentCountry));

            if (!string.Equals(previousCountry, _currentCountry, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(_currentProvince))
            {
                _currentProvince = string.Empty;
                await ProvinceChanged.InvokeAsync(_currentProvince);
            }
        }

        private void OnLatitudeChanged(string value)
        {
            _latitude = value;
            if (!string.IsNullOrWhiteSpace(_latitudeValidationError))
            {
                _latitudeValidationError = string.Empty;
            }
        }

        private void OnLongitudeChanged(string value)
        {
            _longitude = value;
            if (!string.IsNullOrWhiteSpace(_longitudeValidationError))
            {
                _longitudeValidationError = string.Empty;
            }
        }

        #endregion

        public bool Validate()
        {
            if (Required && string.IsNullOrWhiteSpace(_currentAddress))
            {
                _validationError = _localizer["AddressInput_Error_AddressRequired"].Value;
                return false;
            }

            if (!_allowManualAddressEntry && !_apiError && Required && !string.IsNullOrWhiteSpace(_currentAddress) && !_selectedFromSuggestions)
            {
                _validationError = _localizer["AddressInput_Error_SelectFromSuggestions"].Value;
                return false;
            }

            _validationError = string.Empty;
            return true;
        }

        public void SetValidationError(string? message)
        {
            _validationError = message ?? string.Empty;
            StateHasChanged();
        }

        public void ClearValidationError()
        {
            if (string.IsNullOrWhiteSpace(_validationError))
                return;

            _validationError = string.Empty;
            StateHasChanged();
        }

        public void SetFieldValidationErrors(string? unitError, string? postalCodeError, string? stateError = null, string? countryError = null)
        {
            _unitValidationError = unitError ?? string.Empty;
            _postalCodeValidationError = postalCodeError ?? string.Empty;
            _stateValidationError = stateError ?? string.Empty;
            _countryValidationError = countryError ?? string.Empty;
            StateHasChanged();
        }

        public void ClearFieldValidationErrors()
        {
            var hasErrors = !string.IsNullOrWhiteSpace(_unitValidationError)
                || !string.IsNullOrWhiteSpace(_postalCodeValidationError)
                || !string.IsNullOrWhiteSpace(_stateValidationError)
                || !string.IsNullOrWhiteSpace(_countryValidationError);

            if (!hasErrors)
                return;

            _unitValidationError = string.Empty;
            _postalCodeValidationError = string.Empty;
            _stateValidationError = string.Empty;
            _countryValidationError = string.Empty;
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            _addressSearchCancellationTokenSource?.Cancel();
            _addressSearchCancellationTokenSource?.Dispose();
            _addressSearchCancellationTokenSource = null;

            _dotNetRef?.Dispose();

            if (_jsInitialized && _JS != null)
            {
                try
                {
                    await _JS.InvokeVoidAsync("addressInterop.unregisterInput", AddressInputElementId);
                }
                catch (JSDisconnectedException)
                {
                    // Ignored
                }
            }
        }
    }
}
