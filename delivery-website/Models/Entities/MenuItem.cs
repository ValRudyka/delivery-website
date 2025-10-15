namespace delivery_website.Models.Entities
{
    public class MenuItem : BaseEntity
    {
        public Guid MenuItemId { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public int? PreparationTime { get; set; } // in minutes
        public int? Calories { get; set; }

        // Dietary Information (stored as JSON)
        public string? Allergens { get; set; } // JSON array: ["nuts", "dairy"]
        public string? NutritionalInfo { get; set; } // JSON object: {"protein": 20}
        public string? DietaryTags { get; set; } // JSON array: ["vegetarian", "vegan"]

        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
