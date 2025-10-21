namespace delivery_website.Models.DTOs.MenuItem
{
    public class MenuItemDto
    {
        public Guid MenuItemId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        // Additional Information
        public int? PreparationTime { get; set; } // in minutes
        public int? Calories { get; set; }

        public string? Allergens { get; set; } // JSON array: ["nuts", "dairy"]
        public string? DietaryTags { get; set; } // JSON array: ["vegetarian", "vegan"]
    }
}