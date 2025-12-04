namespace delivery_website.Models.DTOs.Admin
{
    public class AdminRestaurantListDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string CuisineType { get; set; } = null!;
        public string OwnerEmail { get; set; } = null!;
        public string OwnerId { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class AdminRestaurantDetailsDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CuisineType { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullAddress { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? DeliveryFee { get; set; }
        public int? EstimatedDeliveryTime { get; set; }
        public string? OpeningHours { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // Owner Information
        public string OwnerId { get; set; } = null!;
        public string OwnerEmail { get; set; } = null!;
        public string OwnerName { get; set; } = null!;

        // Statistics
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int MenuItemsCount { get; set; }
        public int CategoriesCount { get; set; }
    }

    public class UpdateRestaurantStatusDto
    {
        public Guid RestaurantId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsApproved { get; set; }
    }

    public class RestaurantFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? CuisineType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsApproved { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
