using System.ComponentModel.DataAnnotations;

namespace PruebasGraficas.Classes.Models.Employee
{
    public sealed class Employee : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        [Required]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenceId { get; set; }

        [Phone]
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public int AddressId { get; set; }

        [Required]
        public required LanguagesEnum PlatformLanguage { get; init; }

        [Required]
        public required TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;


        public bool EnforceEmailVerification { get; set; }

        public bool IpBlockExemption { get; set; }

        //public IReadOnlyCollection<IpAddress> IpWhitelist => _ipWhitelist.AsReadOnly();

        public bool ChatVisibility { get; set; }

        public bool ChatAccess { get; set; }

        public bool AccessCustomerSMS { get; set; }


        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;


        public string Branch { get; set; } = string.Empty;

        public string LastActive { get; set; } = string.Empty;

        public string ApiId { get; set; } = string.Empty;

        public string TimeZoneId { get; set; } = "UTC";

    }

}
