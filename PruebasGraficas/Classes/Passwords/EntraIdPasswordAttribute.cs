using System.ComponentModel.DataAnnotations;

namespace PruebasGraficas.Classes.Passwords
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class EntraIdPasswordAttribute : ValidationAttribute
    {
        private const int MinLength = 10;
        private const int MaxLength = 256;
        private const int RequiredCategoryCount = 4;
        private const string AllowedSymbols = @"-@#$%^&*_!+=[]{}|\:',. ?/`~""();<>";

        public EntraIdPasswordAttribute()
            : base("Password does not meet Microsoft Entra ID complexity requirements.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Null/empty passwords are allowed (field is nullable/optional); use [Required] to enforce presence.
            if (value is not string password || string.IsNullOrEmpty(password))
            {
                return ValidationResult.Success;
            }

            if (password.Length < MinLength)
            {
                return new ValidationResult($"Password must be at least {MinLength} characters.");
            }

            if (password.Length > MaxLength)
            {
                return new ValidationResult($"Password must not exceed {MaxLength} characters.");
            }

            if (!ContainsOnlyAllowedCharacters(password))
            {
                return new ValidationResult("Password contains invalid characters. Only ASCII letters, digits, and symbols are allowed.");
            }

            if (!MeetsComplexityRequirement(password))
            {
                return new ValidationResult(
                    "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one symbol.");
            }

            return ValidationResult.Success;
        }

        private static bool ContainsOnlyAllowedCharacters(string password)
        {
            foreach (var c in password)
            {
                if (char.IsAsciiLetterUpper(c)) continue;
                if (char.IsAsciiLetterLower(c)) continue;
                if (char.IsAsciiDigit(c)) continue;
                if (AllowedSymbols.Contains(c)) continue;
                return false;
            }

            return true;
        }

        private static bool MeetsComplexityRequirement(string password)
        {
            var categories = 0;

            if (password.Any(char.IsAsciiLetterUpper)) categories++;
            if (password.Any(char.IsAsciiLetterLower)) categories++;
            if (password.Any(char.IsAsciiDigit)) categories++;
            if (password.Any(c => AllowedSymbols.Contains(c))) categories++;

            return categories >= RequiredCategoryCount;
        }
    }
}
