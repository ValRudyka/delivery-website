using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace delivery_website.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class RestaurantsController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantsController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        // GET: /Customer/Restaurants
        public async Task<IActionResult> Index(
            string? cuisineType,
            string? searchTerm,
            decimal? minRating,
            int? maxDeliveryTime,
            string? sortBy)
        {
            var restaurants = await _restaurantService.GetAllRestaurantsAsync(
                cuisineType, searchTerm, minRating, maxDeliveryTime);

            // Apply additional sorting
            restaurants = sortBy switch
            {
                "name" => restaurants.OrderBy(r => r.Name),
                "rating" => restaurants.OrderByDescending(r => r.AverageRating),
                "deliveryTime" => restaurants.OrderBy(r => r.EstimatedDeliveryTime),
                "deliveryFee" => restaurants.OrderBy(r => r.DeliveryFee),
                _ => restaurants.OrderByDescending(r => r.AverageRating)
            };

            var cuisineTypes = await _restaurantService.GetCuisineTypesAsync();

            ViewBag.CuisineTypes = cuisineTypes;
            ViewBag.SelectedCuisine = cuisineType;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinRating = minRating;
            ViewBag.MaxDeliveryTime = maxDeliveryTime;
            ViewBag.SortBy = sortBy;

            return View(restaurants);
        }

        // GET: /Customer/Restaurants/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var restaurant = await _restaurantService.GetRestaurantDetailsAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }
    }
}