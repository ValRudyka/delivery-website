namespace delivery_website.Models.DTOs.Address
{
    public class AddressDto
    {
        public Guid AddressId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public bool IsDefault { get; set; }
        public string? Label { get; set; } // "Home", "Work", etc.
        public string FullAddress => $"{AddressLine1}, {City}, {PostalCode}";
    }
}