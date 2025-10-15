namespace delivery_website.Models.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string? Customizations { get; set; }

        public virtual Cart Cart { get; set; } = null!;
        public virtual MenuItem MenuItem { get; set; } = null!;

        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
