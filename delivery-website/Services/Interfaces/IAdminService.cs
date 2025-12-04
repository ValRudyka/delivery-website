using delivery_website.Models.DTOs.Admin;

namespace delivery_website.Services.Interfaces
{
    public interface IAdminService
    {
        // Dashboard
        Task<AdminDashboardDto> GetDashboardDataAsync();

        // User Management
        Task<PaginatedResult<UserListDto>> GetUsersAsync(UserFilterDto filter);
        Task<UserDetailsDto?> GetUserDetailsAsync(string userId);
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
        Task<bool> LockUserAsync(string userId, int? durationInDays = null);
        Task<bool> UnlockUserAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);

        // Restaurant Management
        Task<PaginatedResult<AdminRestaurantListDto>> GetRestaurantsAsync(RestaurantFilterDto filter);
        Task<AdminRestaurantDetailsDto?> GetRestaurantDetailsAsync(Guid restaurantId);
        Task<bool> ApproveRestaurantAsync(Guid restaurantId);
        Task<bool> RejectRestaurantAsync(Guid restaurantId);
        Task<bool> ActivateRestaurantAsync(Guid restaurantId);
        Task<bool> DeactivateRestaurantAsync(Guid restaurantId);
        Task<bool> DeleteRestaurantAsync(Guid restaurantId);

        // Order Management
        Task<PaginatedResult<AdminOrderListDto>> GetOrdersAsync(OrderFilterDto filter);
        Task<AdminOrderDetailsDto?> GetOrderDetailsAsync(Guid orderId);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? cancellationReason = null);

        // Review Management
        Task<PaginatedResult<AdminReviewListDto>> GetReviewsAsync(ReviewFilterDto filter);
        Task<AdminReviewDetailsDto?> GetReviewDetailsAsync(Guid reviewId);
        Task<bool> ApproveReviewAsync(Guid reviewId, string moderatorId, string? notes = null);
        Task<bool> RejectReviewAsync(Guid reviewId, string moderatorId, string? notes = null);
        Task<bool> DeleteReviewAsync(Guid reviewId);
    }
}
