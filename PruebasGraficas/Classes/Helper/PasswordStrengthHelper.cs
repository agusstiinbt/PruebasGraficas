using MudBlazor;
using PruebasGraficas.Classes.Passwords;

namespace CigoWeb.Core.Helpers;

public static class PasswordStrengthHelper
{
    private const int minLength = 8;
    private const int maxLength = 256;
    private const string allowedSymbols = @"-@#$%^&*_!+=[]{}|\:',. ?/`~""();<>";

    public static PasswordStrengthResult Calculate(string? password, IStringLocalizer<LocalResources> localizer)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return new PasswordStrengthResult(false, 0, Color.Error, string.Empty);
        }

        int types = GetCharacterTypeCount(password);
        bool valid = types >= 4;
        double percent = (types / 4.0) * 100;

        if (password.Length < minLength || password.Length > maxLength)
        {
            return new PasswordStrengthResult(false, percent, Color.Error, localizer["Password_Strength_Result_Label_Weak"]);
        }

        if (HasInvalidCharacters(password))
        {
            return new PasswordStrengthResult(false, percent, Color.Error, localizer["Password_Strength_Result_Label_Weak"]);
        }

        if (types == 3)
        {
            return new PasswordStrengthResult(valid, percent, Color.Warning, localizer["Password_Strength_Result_Label_Strong"]);
        }

        if (!valid)
        {
            return new PasswordStrengthResult(valid, percent, Color.Error, localizer["Password_Strength_Result_Label_Weak"]);
        }

        return new PasswordStrengthResult(valid, percent, Color.Success, localizer["Password_Strength_Result_Label_Very_Strong"]);
    }

    public static string? GetValidationMessage(string? password, IStringLocalizer<LocalResources> localizer)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return localizer["Password_Validation_Password_Required"];
        }

        if (password.Length < minLength || password.Length > maxLength)
        {
            return localizer["Password_Validation_Length"];
        }

        if (HasInvalidCharacters(password))
        {
            return localizer["Password_Validation_InvalidCharacters"];
        }

        int types = GetCharacterTypeCount(password);

        if (types < 4)
        {
            return localizer["Validation.PasswordComplexity"];
        }

        return null;
    }

    private static bool HasInvalidCharacters(string password)
    {
        foreach (var c in password)
        {
            if (char.IsAsciiLetterUpper(c)) continue;
            if (char.IsAsciiLetterLower(c)) continue;
            if (char.IsAsciiDigit(c)) continue;
            if (allowedSymbols.Contains(c)) continue;

            return true;
        }

        return false;
    }

    private static int GetCharacterTypeCount(string password)
    {
        int types = 0;
        if (password.Any(char.IsAsciiLetterLower)) types++;
        if (password.Any(char.IsAsciiLetterUpper)) types++;
        if (password.Any(char.IsAsciiDigit)) types++;
        if (password.Any(c => allowedSymbols.Contains(c))) types++;

        return types;
    }
}
