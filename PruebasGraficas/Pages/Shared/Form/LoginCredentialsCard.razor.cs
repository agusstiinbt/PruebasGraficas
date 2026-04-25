using System.Linq.Expressions;
using MudBlazor;
using PruebasGraficas.Classes.Passwords;

namespace PruebasGraficas.Pages.Shared.Form;
public partial class LoginCredentialsCard
{
    [Inject] private IStringLocalizer<LocalResources> Localizer { get; init; } = default!;

    [Parameter] public string CardClass { get; set; } = "custom-card mb-6";
    [Parameter] public bool Required { get; set; }

    [Parameter] public string LoginEmail { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> LoginEmailChanged { get; set; }
    [Parameter] public Expression<Func<string>>? ForLoginEmail { get; set; }

    [Parameter] public string Password { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> PasswordChanged { get; set; }
    [Parameter] public Expression<Func<string>>? ForPassword { get; set; }

    [Parameter] public string ConfirmPassword { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ConfirmPasswordChanged { get; set; }
    [Parameter] public Expression<Func<string>>? ForConfirmPassword { get; set; }

    [Parameter] public EventCallback PasswordValueChangedAfter { get; set; }

    private PasswordVisibility _passwordVis = new(false, InputType.Password, Icons.Material.Filled.VisibilityOff);
    private PasswordVisibility _confirmPasswordVis = new(false, InputType.Password, Icons.Material.Filled.VisibilityOff);
    private PasswordStrengthResult _passwordStrength = new(false, 0, Color.Error, string.Empty);

    protected override void OnParametersSet()
    {
        _passwordStrength = PasswordStrengthHelper.Calculate(Password, Localizer);
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

    private async Task HandleLoginEmailChangedAsync(string value)
    {
        if (LoginEmail == value)
            return;

        LoginEmail = value;
        await LoginEmailChanged.InvokeAsync(value);
    }

    private async Task HandlePasswordChangedAsync(string value)
    {
        if (Password == value)
            return;

        Password = value;
        _passwordStrength = PasswordStrengthHelper.Calculate(Password, Localizer);

        await PasswordChanged.InvokeAsync(value);

        if (PasswordValueChangedAfter.HasDelegate)
            await PasswordValueChangedAfter.InvokeAsync();
    }

    private async Task HandleConfirmPasswordChangedAsync(string value)
    {
        if (ConfirmPassword == value)
            return;

        ConfirmPassword = value;
        await ConfirmPasswordChanged.InvokeAsync(value);
    }
}
