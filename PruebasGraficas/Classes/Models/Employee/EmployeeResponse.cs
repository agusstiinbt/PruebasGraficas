using PruebasGraficas.Classes.Models.Address;
using PruebasGraficas.Classes.Models.IpList;
using PruebasGraficas.Classes.Models.Location;

namespace PruebasGraficas.Classes.Models.Employee
{
    public class EmployeeResponse : BaseResponse
    {
        public EmployeeRole Role { get; set; }

        public int? LocationId { get; set; }

        public LocationResponse? Location { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ReferenceId { get; set; }

        public string? PhoneNumber { get; set; } = string.Empty;
        public string? PhoneNumberIsoCode { get; set; } = string.Empty;

        public Languages PlatformLanguage { get; set; }

        public string TimeZoneId { get; set; } = "UTC";

        public bool IpBlockExemption { get; set; }

        public bool ChatVisibility { get; set; }

        public bool ChatAccess { get; set; }

        public bool AccessCustomerSMS { get; set; }

        public AddressResponse? Address { get; set; } = null;

        public IReadOnlyCollection<IpWhitelistItemResponse> IpWhitelist { get; set; } = Array.Empty<IpWhitelistItemResponse>();
    }
}
