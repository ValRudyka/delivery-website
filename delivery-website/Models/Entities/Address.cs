namespace delivery_website.Models.Entities
{
    public class Address : BaseEntity
    {
        public Guid AddressId { get; set; }
        public string UserId { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string? State { get; set; }
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = "Ukraine";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; }
        public string? Label { get; set; }
        public string FullAddress => $"{AddressLine1}, {City}, {PostalCode}";
    }
}
