using PruebasGraficas.Classes.Models;

namespace PruebasGraficas.Classes.Validator.Employee
{

    public sealed class EmployeeCreateEditModel : BaseEntity, IHasCountryPhoneInfo
    {
        public int? LocationId { get; set; }

        /// <summary>
        /// It stores the integer value (converted to a string for better display) of the EnumRole selected
        /// </summary>
        public string Role { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        #region PhoneNumber
        public string? PhoneNumber { get; set; } = string.Empty;
        public CountryPhoneInfo? CountryPhoneInfo { get; set; }
        #endregion

        #region Address

        public string AddressStreet { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ProvinceState { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// It stores the integer value (converted to a string for better display) of the Enum selected
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// This is better to be a string because otherwise it will show zero (0) in the frontend
        /// </summary>
        public string Latitude { get; set; } = string.Empty;

        /// <summary>
        /// This is better to be a string because otherwise it will show zero (0) in the frontend
        /// </summary>
        public string Longitude { get; set; } = string.Empty;
        public bool AllowManualAddressEntry { get; set; }

        #endregion

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

    }

}
