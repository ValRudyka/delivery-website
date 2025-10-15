namespace delivery_website.Models.Entities
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        // Values: CreditCard, DebitCard, DigitalWallet, CashOnDelivery
        public string PaymentStatus { get; set; } = "Pending";
        // Values: Pending, Processing, Completed, Failed, Refunded, Cancelled
        public decimal Amount { get; set; }

        public string PaymentGateway { get; set; } = null!;
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        public bool IsRefunded { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundTransactionId { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
