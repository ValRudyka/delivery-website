namespace delivery_website.Models.DTOs.UserProfile
{
    public class UserProfileDto
    {
        public Guid UserProfileId { get; set; }
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string PreferredLanguage { get; set; } = "uk";
        public string FullName => $"{FirstName} {LastName}";
        public DateTime CreatedDate { get; set; }
    }
}