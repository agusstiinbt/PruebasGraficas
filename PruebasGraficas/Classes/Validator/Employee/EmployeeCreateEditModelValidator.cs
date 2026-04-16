namespace PruebasGraficas.Classes.Validator.Employee
{
    public class EmployeeCreateEditModelValidator : AbstractValidator<EmployeeCreateEditModel>
    {
        public EmployeeCreateEditModelValidator(IStringLocalizer<LocalResources> localizer)
        {

            #region Primary Information

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage(localizer["Role_Is_Required"])
                .NotNull()
                .WithMessage(localizer["Role_Cant_Be_Null"]);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(localizer["First_Name_Cannot_Be_Empty"])
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(localizer["Last_Name_Cannot_Be_Empty"])
                .MaximumLength(100);

            #region PhoneNumber

            RuleFor(x => x.PhoneNumber)
                    .Custom((phoneNumber, context) =>
                    {
                        var model = context.InstanceToValidate;

                        if (string.IsNullOrWhiteSpace(phoneNumber))
                            return;

                        if (model.CountryPhoneInfo == null || string.IsNullOrWhiteSpace(model.CountryPhoneInfo.DialCode))
                        {
                            context.AddFailure(nameof(model.CountryPhoneInfo), localizer["CountryCode_Is_Necessary"]);
                            return;
                        }

                        //var validationMessage = PhoneNumberHelper.GetValidationMessage(
                        //    model.CountryPhoneInfo,
                        //    phoneNumber,
                        //    localizer);

                        //if (!string.IsNullOrWhiteSpace(validationMessage))
                        //{
                        //    context.AddFailure(nameof(model.PhoneNumber), validationMessage);
                        //}
                    });

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizer["PhoneNumber_Is_Necessary"])
                .When(x => x.CountryPhoneInfo != null);

            #endregion

            #region Address
            //this will be added in another story, we will start with the primary information and then we will add the address validation later
            //RuleFor(x => x.AddressStreet)
            //	.NotNull()
            //	.WithMessage("Street address cant be null")
            //	.NotEmpty()
            //	.WithMessage("Street is required")
            //	.MaximumLength(200);

            //RuleFor(x => x.UnitNumber)
            //	.NotEmpty()
            //	.NotNull()
            //	.MinimumLength(2)
            //	.MaximumLength(15);

            //RuleFor(x => x.PostalCode)
            //	.NotEmpty()
            //	.NotNull()
            //	.MinimumLength(4)
            //	.MaximumLength(10);

            //RuleFor(x => x.ProvinceState)
            //	.NotEmpty()
            //	.NotNull()
            //	.MaximumLength(100);

            //RuleFor(x => x.City)
            //	.NotEmpty()
            //	.NotNull()
            //	.MaximumLength(100);

            //RuleFor(x => x.Country)
            // .NotEmpty()
            // .NotNull();

            //RuleFor(x => x.Latitude)
            //	.NotNull()
            //	.NotEmpty();

            //RuleFor(x => x.Longitude)
            //	.NotNull()
            //	.NotEmpty();

            #endregion

            #endregion

            #region Login Credentials

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizer["Email_Is_Required"])
                .EmailAddress()
                .WithMessage(localizer["Please_Enter_Valid_Email_Address"])
                .Must(email => EmailHelper.GetValidateEmailMessage(email, localizer) == null)
                .WithMessage(x => EmailHelper.GetValidateEmailMessage(x.Email, localizer)!)
                .MaximumLength(100);


            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(localizer["Password_Validation_Password_Required"])
                .Must(password => PasswordStrengthHelper.GetValidationMessage(password, localizer) == null)
                .WithMessage(x => PasswordStrengthHelper.GetValidationMessage(x.Password, localizer)!)
                .When(x => x.Id == 0 || !string.IsNullOrWhiteSpace(x.Password));


            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage(localizer["Password_Validation_ConfirmPassword_Required"])
                .Equal(x => x.Password)
                .WithMessage(localizer["Password_Validation_ConfirmPassword_MustMatch"])
                .When(x => x.Id == 0 || !string.IsNullOrWhiteSpace(x.Password));

            #endregion

        }
    }
}
