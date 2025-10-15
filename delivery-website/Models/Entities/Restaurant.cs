using System.Text.Json;

namespace delivery_website.Models.Entities
{
    public class Restaurant : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public string OwnerId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CuisineType { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string? State { get; set; }
        public string PostalCode { get; set; } = null!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }

        public decimal? MinimumOrderAmount { get; set; }
        public decimal? DeliveryFee { get; set; }
        public int? EstimatedDeliveryTime { get; set; } // in minutes
        public decimal? DeliveryRadius { get; set; } // in kilometers

        // Opening Hours (stored as JSON)
        public string? OpeningHours { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public DateTime? ApprovedDate { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public string FullAddress => $"{AddressLine1}, {City}, {PostalCode}";
    }
}
