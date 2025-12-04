namespace delivery_website.Models.DTOs.Admin
{
    public class AdminOrderListDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string RestaurantName { get; set; } = null!;
        public Guid RestaurantId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public int ItemsCount { get; set; }
    }

    public class AdminOrderDetailsDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string OrderStatus { get; set; } = null!;

        // Customer Information
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;

        // Restaurant Information
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = null!;
        public string RestaurantPhone { get; set; } = null!;

        // Delivery Information
        public string? DeliveryAddress { get; set; }
        public string? DeliveryInstructions { get; set; }

        // Pricing
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Dates
        public DateTime OrderDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? PreparingDate { get; set; }
        public DateTime? ReadyDate { get; set; }
        public DateTime? OutForDeliveryDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string? CancellationReason { get; set; }

        // Payment Information
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TransactionId { get; set; }

        // Order Items
        public List<AdminOrderItemDto> Items { get; set; } = new();
    }

    public class AdminOrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Customizations { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; } = null!;
        public string? CancellationReason { get; set; }
    }

    public class OrderFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? OrderStatus { get; set; }
        public Guid? RestaurantId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PaymentStatus { get; set; }
        public string? SortBy { get; set; } = "OrderDate";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
