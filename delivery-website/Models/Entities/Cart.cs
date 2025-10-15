namespace delivery_website.Models.Entities
{
    public class Cart : BaseEntity
    {
        public Guid CartId { get; set; }
        public string UserId { get; set; } = null!;
        public Guid RestaurantId { get; set; }
        public DateTime ExpiresAt { get; set; }

        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
    }
}
