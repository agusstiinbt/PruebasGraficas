using MudBlazor;

namespace PruebasGraficas.Classes.Passwords;

public record Password(bool Valid, double Value, Color Color, string Label);

public record PasswordVisibility(bool IsVisible, InputType InputType, string Icon);

public record PasswordStrengthResult(bool Valid, double Value, Color Color, string Label);
