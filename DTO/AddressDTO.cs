namespace RideWild.DTO
{
    public class AddressDTO
    {
        
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string StateProvince { get; set; } = null!;
        public string CountryRegion { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string AddressType { get; set; } = null!;

    }
}
