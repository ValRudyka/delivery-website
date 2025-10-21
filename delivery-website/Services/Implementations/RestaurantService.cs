using delivery_website.Models.DTOs.MenuItem;
using delivery_website.Models.DTOs.Restaurant;
using delivery_website.Repositories.Interfaces;
using delivery_website.Services.Interfaces;

namespace delivery_website.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<IEnumerable<RestaurantListDto>> GetAllRestaurantsAsync(
            string? cuisineType = null,
            string? searchTerm = null,
            decimal? minRating = null,
            int? maxDeliveryTime = null)
        {
            var restaurants = await _restaurantRepository.GetAllRestaurantsAsync(
                cuisineType, searchTerm, minRating, maxDeliveryTime);

            return restaurants.Select(r => new RestaurantListDto
            {
                RestaurantId = r.RestaurantId,
                Name = r.Name,
                Description = r.Description,
                CuisineType = r.CuisineType,
                LogoUrl = r.LogoUrl,
                CoverImageUrl = r.CoverImageUrl,
                MinimumOrderAmount = r.MinimumOrderAmount,
                DeliveryFee = r.DeliveryFee,
                EstimatedDeliveryTime = r.EstimatedDeliveryTime,
                AverageRating = r.AverageRating,
                TotalReviews = r.TotalReviews,
                IsActive = r.IsActive,
                FullAddress = r.FullAddress
            });
        }

        public async Task<RestaurantDetailsDto?> GetRestaurantDetailsAsync(Guid restaurantId)
        {
            var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(restaurantId, includeMenuItems: true);

            if (restaurant == null)
                return null;

            return new RestaurantDetailsDto
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Description = restaurant.Description,
                CuisineType = restaurant.CuisineType,
                PhoneNumber = restaurant.PhoneNumber,
                Email = restaurant.Email,
                AddressLine1 = restaurant.AddressLine1,
                AddressLine2 = restaurant.AddressLine2,
                City = restaurant.City,
                PostalCode = restaurant.PostalCode,
                FullAddress = restaurant.FullAddress,
                LogoUrl = restaurant.LogoUrl,
                CoverImageUrl = restaurant.CoverImageUrl,
                WebsiteUrl = restaurant.WebsiteUrl,
                MinimumOrderAmount = restaurant.MinimumOrderAmount,
                DeliveryFee = restaurant.DeliveryFee,
                EstimatedDeliveryTime = restaurant.EstimatedDeliveryTime,
                OpeningHours = restaurant.OpeningHours,
                AverageRating = restaurant.AverageRating,
                TotalReviews = restaurant.TotalReviews,
                IsActive = restaurant.IsActive,
                Categories = restaurant.Categories
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new CategoryWithItemsDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        SortOrder = c.SortOrder,
                        MenuItems = c.MenuItems
                            .Where(m => m.IsAvailable)
                            .Select(m => new MenuItemDto
                            {
                                MenuItemId = m.MenuItemId,
                                Name = m.Name,
                                Description = m.Description,
                                Price = m.Price,
                                ImageUrl = m.ImageUrl,
                                IsAvailable = m.IsAvailable,
                                PreparationTime = m.PreparationTime,
                                Calories = m.Calories,
                                Allergens = m.Allergens,
                                DietaryTags = m.DietaryTags
                            }).ToList()
                    }).ToList()
            };
        }

        public async Task<IEnumerable<string>> GetCuisineTypesAsync()
        {
            return await _restaurantRepository.GetUniqueCuisineTypesAsync();
        }
    }
}