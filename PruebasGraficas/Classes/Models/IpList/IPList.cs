namespace PruebasGraficas.Classes.Models.IpList
{
    public class IpWhitelistItemResponse
    {
        public int Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class IpWhitelistItemRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

}
