namespace delivery_website.Models.DTOs.Cart
{
    public class AddToCartDto
    {
        public Guid MenuItemId { get; set; }
        public Guid RestaurantId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Customizations { get; set; }
    }
}
