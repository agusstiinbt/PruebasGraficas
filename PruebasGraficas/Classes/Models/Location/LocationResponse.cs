using PruebasGraficas.Classes.Models.Address;

namespace PruebasGraficas.Classes.Models.Location
{
    public class LocationResponse : BaseResponse
    {
        public LocationType Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ReferenceId { get; set; }

        public string? CustomerServicePhoneNumber { get; set; }

        public string? CustomerServiceEmail { get; set; }

        public AddressResponse Address { get; set; } = new();

        public string? FacebookId { get; set; }

        public string? GooglePlaceId { get; set; }

        public string? XUsername { get; set; }

        public IReadOnlyCollection<string> XHashtags { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<string> MailingListNegativeReviews { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<string> MailingListIncompleteJobs { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<string> MailingListDamagedJobs { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<string> MailingListCustomerReplies { get; set; } = Array.Empty<string>();

    }

    public enum LocationType
    {
        Branch = 1,
        Warehouse = 2
    }
}
