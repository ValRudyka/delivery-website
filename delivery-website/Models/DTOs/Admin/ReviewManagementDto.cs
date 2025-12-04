namespace delivery_website.Models.DTOs.Admin
{
    public class AdminReviewListDto
    {
        public Guid ReviewId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string RestaurantName { get; set; } = null!;
        public Guid RestaurantId { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; } = null!;
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModerationDate { get; set; }
        public string? ModeratorName { get; set; }
    }

    public class AdminReviewDetailsDto
    {
        public Guid ReviewId { get; set; }
        
        // Customer Information
        public string UserId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;

        // Restaurant Information
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = null!;

        // Order Information (if verified purchase)
        public Guid? OrderId { get; set; }
        public string? OrderNumber { get; set; }

        // Review Content
        public int Rating { get; set; }
        public string ReviewText { get; set; } = null!;
        public List<string>? ImageUrls { get; set; }
        public bool IsVerifiedPurchase { get; set; }

        // Moderation
        public bool IsApproved { get; set; }
        public DateTime? ModerationDate { get; set; }
        public string? ModeratorId { get; set; }
        public string? ModeratorName { get; set; }
        public string? ModerationNotes { get; set; }

        // Dates
        public DateTime CreatedDate { get; set; }
    }

    public class ModerateReviewDto
    {
        public Guid ReviewId { get; set; }
        public bool IsApproved { get; set; }
        public string? ModerationNotes { get; set; }
    }

    public class ReviewFilterDto
    {
        public string? SearchTerm { get; set; }
        public Guid? RestaurantId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsVerifiedPurchase { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
