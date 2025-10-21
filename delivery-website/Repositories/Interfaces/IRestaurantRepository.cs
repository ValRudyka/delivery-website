using delivery_website.Models.Entities;

namespace delivery_website.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync(
            string? cuisineType = null,
            string? searchTerm = null,
            decimal? minRating = null,
            int? maxDeliveryTime = null,
            bool activeOnly = true);

        Task<Restaurant?> GetRestaurantByIdAsync(Guid restaurantId, bool includeMenuItems = false);

        Task<IEnumerable<string>> GetUniqueCuisineTypesAsync();

        Task<bool> RestaurantExistsAsync(Guid restaurantId);

        Task<Restaurant> CreateRestaurantAsync(Restaurant restaurant);

        Task<Restaurant> UpdateRestaurantAsync(Restaurant restaurant);

        Task DeleteRestaurantAsync(Guid restaurantId);

        Task<IEnumerable<Restaurant>> GetRestaurantsByOwnerIdAsync(string ownerId);
    }
}