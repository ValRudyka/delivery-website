using delivery_website.Models.Entities;

namespace delivery_website.Models.Entities
{
    public class Category : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
