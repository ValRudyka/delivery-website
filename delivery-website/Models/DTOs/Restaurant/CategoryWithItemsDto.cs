using delivery_website.Models.DTOs.MenuItem;

namespace delivery_website.Models.DTOs.Restaurant
{
    public class CategoryWithItemsDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public List<MenuItemDto> MenuItems { get; set; } = new();
    }
}