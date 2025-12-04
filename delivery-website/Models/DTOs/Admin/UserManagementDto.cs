namespace delivery_website.Models.DTOs.Admin
{
    public class UserListDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsEmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrdersCount { get; set; }
    }

    public class UserDetailsDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string PreferredLanguage { get; set; } = "uk";
        public string Role { get; set; } = null!;
        public bool IsEmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public int ReviewsCount { get; set; }
        public int AddressesCount { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class UpdateUserRoleDto
    {
        public string UserId { get; set; } = null!;
        public string NewRole { get; set; } = null!;
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsLockedOut { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
