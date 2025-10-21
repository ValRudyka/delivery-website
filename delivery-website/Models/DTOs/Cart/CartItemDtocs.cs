namespace delivery_website.Models.DTOs.Cart
{
    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public string? MenuItemImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Customizations { get; set; }
    }
}
