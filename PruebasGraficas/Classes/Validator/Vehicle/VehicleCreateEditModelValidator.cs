namespace PruebasGraficas.Classes.Validator.Vehicle;

public class VehicleCreateEditModelValidator : AbstractValidator<VehicleCreateEditModel>
{
    public VehicleCreateEditModelValidator(IStringLocalizer<LocalResources> localizer)
    {
        // Some properties like DepartureTime and VehicleType dont need validations because they have default values and UI doesnt allow clearence


        #region Primary Information

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizer["Vehicle_Name_Is_Required"])
            .NotNull()
            .MaximumLength(200)
            .WithMessage(localizer["Vehicle_Name_Length_Validation"]);

        RuleFor(x => x.LicensePlate)
            .MaximumLength(50)
            .WithMessage(localizer["Vehicle_License_Plate_Length_Validation"])
            .When(x => !string.IsNullOrEmpty(x.LicensePlate));

        RuleFor(x => x.VehicleRefId)
            .MaximumLength(100)
            .WithMessage(localizer["Vehicle_Vehicle_Reference_Id_Length_Validation"])
        .When(x => !string.IsNullOrEmpty(x.VehicleRefId));

        RuleFor(x => x.WeightCapacityLbs)
            .InclusiveBetween(0m, 9999999999999999.99m)
            .WithMessage(localizer["Vehicle_Weight_Capacity_Validation_OutOfRange"])
            .When(x => x.WeightCapacityLbs.HasValue && x.WeightCapacityLbs.Value != 0m);

        RuleFor(x => x.VolumeCapacityFt3)
            .InclusiveBetween(0m, 9999999999999999.99m)
            .WithMessage(localizer["Vehicle_Volume_Capacity_Validation_OutOfRange"])
            .When(x => x.VolumeCapacityFt3.HasValue && x.VolumeCapacityFt3 != null && x.VolumeCapacityFt3.Value != 0m);

        RuleFor(x => x.CapacityBufferPercent)
            .InclusiveBetween(0m, 999.99m)
            .WithMessage(localizer["Vehicle_Capacity_Buffer_Percent"])
            .When(x => x.CapacityBufferPercent.HasValue && x.CapacityBufferPercent.Value != 0m);

        #endregion

    }
}


