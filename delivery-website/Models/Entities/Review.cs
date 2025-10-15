namespace delivery_website.Models.Entities
{
    public class Review : BaseEntity
    {
        public Guid ReviewId { get; set; }
        public string UserId { get; set; } = null!;
        public Guid RestaurantId { get; set; }
        public Guid? OrderId { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; } = null!;

        public string? ImageUrls { get; set; }

        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }

        public DateTime? ModerationDate { get; set; }
        public string? ModeratorId { get; set; }
        public string? ModerationNotes { get; set; }

        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual Order? Order { get; set; }
    }
}
