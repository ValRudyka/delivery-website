namespace delivery_website.Models.Entities
{
    public class Order : BaseEntity
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!; //Example: ORD-2025-001234
        public string UserId { get; set; } = null!;
        public Guid RestaurantId { get; set; }

        public string OrderStatus { get; set; } = "Pending";

        public Guid? DeliveryAddressId { get; set; }
        public string? DeliveryInstructions { get; set; }

        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? PreparingDate { get; set; }
        public DateTime? ReadyDate { get; set; }
        public DateTime? OutForDeliveryDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string? CancellationReason { get; set; }

        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual Address? DeliveryAddress { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual Payment? Payment { get; set; }
        public virtual Review? Review { get; set; }
    }
}
