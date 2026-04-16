using FluentValidation.Results;

namespace PruebasGraficas.Classes.Validator
{
    public static class FluentValidationExtensions
    {
        public static Func<object, string, Task<IEnumerable<string>>> ValidateValue<T>(this IValidator<T> validator)
        {
            return async (model, propertyName) =>
            {
                var context = ValidationContext<T>.CreateWithOptions(
                    (T)model,
                    x => x.IncludeProperties(propertyName));

                ValidationResult result = await validator.ValidateAsync(context);

                if (result.IsValid)
                    return Array.Empty<string>();

                return result.Errors.Select(e => e.ErrorMessage);
            };
        }
    }
}
