namespace delivery_website.Models.DTOs.Restaurant
{
    public class RestaurantListDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CuisineType { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? DeliveryFee { get; set; }
        public int? EstimatedDeliveryTime { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; }
        public string FullAddress { get; set; } = null!;
    }
}