namespace PruebasGraficas.Classes.Models.Address
{
    public class AddressRequest : BaseRequest
    {
        public string Street { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public Countries Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
