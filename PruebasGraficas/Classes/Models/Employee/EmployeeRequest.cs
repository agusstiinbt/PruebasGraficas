using System.ComponentModel.DataAnnotations;
using PruebasGraficas.Classes.Models.Address;
using PruebasGraficas.Classes.Models.IpList;

namespace PruebasGraficas.Classes.Models.Employee
{
    public class EmployeeRequest : BaseRequest
    {
        public int? LocationId { get; set; }
        public EmployeeRole Role { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ReferenceId { get; set; }

        public string? PhoneNumber { get; set; } = string.Empty;
        public string? PhoneNumberIsoCode { get; set; } = string.Empty;

        public Languages PlatformLanguage { get; set; }

        /// <summary>
        /// Time zone identifier (OS-specific). Prefer "UTC".
        /// </summary>
        public string TimeZoneId { get; set; } = "UTC";

        public bool IpBlockExemption { get; set; }

        public bool ChatVisibility { get; set; }

        public bool ChatAccess { get; set; }

        public bool AccessCustomerSMS { get; set; }

        public AddressRequest? Address { get; set; } = null;

        public List<IpWhitelistItemRequest> IpWhitelist { get; set; } = [];

        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
