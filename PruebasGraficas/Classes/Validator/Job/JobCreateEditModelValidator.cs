namespace PruebasGraficas.Classes.Validator.Job
{
    public class JobCreateEditModelValidator : AbstractValidator<JobCreateEditModel>
    {
        public JobCreateEditModelValidator(IStringLocalizer<LocalResources> localizer)
        {

            RuleFor(x => x.TotalValue)
                .GreaterThanOrEqualTo(0m)
                .When(x => x.TotalValue.HasValue)
                .WithMessage("Error en nuestra propiedad");

            RuleFor(x => x.TotalWeightLbs)
                .GreaterThanOrEqualTo(0m)
                .When(x => x.TotalWeightLbs.HasValue)
                .WithMessage("Error en nuestra propiedad");

        }
    }
}
