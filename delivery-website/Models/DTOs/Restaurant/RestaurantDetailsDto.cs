namespace delivery_website.Models.DTOs.Restaurant
{
    public class RestaurantDetailsDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CuisineType { get; set; } = null!;

        // Contact Information
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Address Information
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string FullAddress { get; set; } = null!;

        // Images and Branding
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }

        // Order Information
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? DeliveryFee { get; set; }
        public int? EstimatedDeliveryTime { get; set; }

        // Operating Hours (JSON string)
        public string? OpeningHours { get; set; }

        // Ratings and Status
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; }

        // Menu Categories with Items
        public List<CategoryWithItemsDto> Categories { get; set; } = new();
    }
}