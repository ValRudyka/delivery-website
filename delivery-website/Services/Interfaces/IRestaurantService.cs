using delivery_website.Models.DTOs.Restaurant;

namespace delivery_website.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<IEnumerable<RestaurantListDto>> GetAllRestaurantsAsync(
            string? cuisineType = null,
            string? searchTerm = null,
            decimal? minRating = null,
            int? maxDeliveryTime = null);

        Task<RestaurantDetailsDto?> GetRestaurantDetailsAsync(Guid restaurantId);

        Task<IEnumerable<string>> GetCuisineTypesAsync();
    }
}