namespace delivery_website.Models.DTOs.Admin
{
    public class AdminDashboardDto
    {
        // Users Statistics
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalRestaurantOwners { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }

        // Restaurants Statistics
        public int TotalRestaurants { get; set; }
        public int ActiveRestaurants { get; set; }
        public int PendingApprovalRestaurants { get; set; }
        public int InactiveRestaurants { get; set; }

        // Orders Statistics
        public int TotalOrders { get; set; }
        public int OrdersToday { get; set; }
        public int OrdersThisWeek { get; set; }
        public int OrdersThisMonth { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrdersToday { get; set; }
        public int CancelledOrdersToday { get; set; }

        // Revenue Statistics
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Reviews Statistics
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int ApprovedReviews { get; set; }
        public decimal AverageRating { get; set; }

        // Recent Activity
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<RecentUserDto> RecentUsers { get; set; } = new();
        public List<RecentReviewDto> RecentReviews { get; set; } = new();
    }

    public class RecentOrderDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string RestaurantName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
    }

    public class RecentUserDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
    }

    public class RecentReviewDto
    {
        public Guid ReviewId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string RestaurantName { get; set; } = null!;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
