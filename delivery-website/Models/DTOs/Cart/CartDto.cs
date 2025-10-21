namespace delivery_website.Models.DTOs.Cart
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = null!;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubtotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount => Items.Sum(i => i.Quantity);
    }
}
