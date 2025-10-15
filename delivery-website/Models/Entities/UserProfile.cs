namespace delivery_website.Models.Entities
{
    public class UserProfile : BaseEntity
    {
        public Guid UserProfileId { get; set; }
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string PreferredLanguage { get; set; } = "uk";

        public string FullName => $"{FirstName} {LastName}";
    }
}
