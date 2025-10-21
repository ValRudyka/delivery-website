using delivery_website.Data;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace delivery_website.Repositories.Implementations
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly ApplicationDbContext _context;

        public RestaurantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync(
            string? cuisineType = null,
            string? searchTerm = null,
            decimal? minRating = null,
            int? maxDeliveryTime = null,
            bool activeOnly = true)
        {
            var query = _context.Restaurants.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(r => r.IsActive && r.IsApproved);
            }

            if (!string.IsNullOrWhiteSpace(cuisineType))
            {
                query = query.Where(r => r.CuisineType == cuisineType);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    r.Description.ToLower().Contains(searchTerm) ||
                    r.CuisineType.ToLower().Contains(searchTerm));
            }

            if (minRating.HasValue)
            {
                query = query.Where(r => r.AverageRating >= minRating.Value);
            }

            if (maxDeliveryTime.HasValue)
            {
                query = query.Where(r => r.EstimatedDeliveryTime <= maxDeliveryTime.Value);
            }

            return await query
                .OrderByDescending(r => r.AverageRating)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Restaurant?> GetRestaurantByIdAsync(Guid restaurantId, bool includeMenuItems = false)
        {
            var query = _context.Restaurants.AsQueryable();

            if (includeMenuItems)
            {
                query = query
                    .Include(r => r.Categories.Where(c => c.IsActive))
                        .ThenInclude(c => c.MenuItems.Where(m => m.IsActive))
                    .Include(r => r.Reviews.Where(rv => rv.IsApproved));
            }

            return await query.FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
        }

        public async Task<IEnumerable<string>> GetUniqueCuisineTypesAsync()
        {
            return await _context.Restaurants
                .Where(r => r.IsActive && r.IsApproved)
                .Select(r => r.CuisineType)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<bool> RestaurantExistsAsync(Guid restaurantId)
        {
            return await _context.Restaurants.AnyAsync(r => r.RestaurantId == restaurantId);
        }

        public async Task<Restaurant> CreateRestaurantAsync(Restaurant restaurant)
        {
            restaurant.CreatedDate = DateTime.UtcNow;
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task<Restaurant> UpdateRestaurantAsync(Restaurant restaurant)
        {
            restaurant.UpdatedDate = DateTime.UtcNow;
            _context.Restaurants.Update(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task DeleteRestaurantAsync(Guid restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant != null)
            {
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsByOwnerIdAsync(string ownerId)
        {
            return await _context.Restaurants
                .Where(r => r.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}