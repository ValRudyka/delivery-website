using delivery_website.Models.DTOs.Admin;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace delivery_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RestaurantsController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IRestaurantService _restaurantService;
        private readonly ILogger<RestaurantsController> _logger;

        public RestaurantsController(
            IAdminService adminService,
            IRestaurantService restaurantService,
            ILogger<RestaurantsController> logger)
        {
            _adminService = adminService;
            _restaurantService = restaurantService;
            _logger = logger;
        }

        // GET: /Admin/Restaurants
        public async Task<IActionResult> Index(RestaurantFilterDto filter)
        {
            try
            {
                var restaurants = await _adminService.GetRestaurantsAsync(filter);
                var cuisineTypes = await _restaurantService.GetCuisineTypesAsync();
                
                ViewBag.Filter = filter;
                ViewBag.CuisineTypes = cuisineTypes;
                
                return View(restaurants);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading restaurants: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження списку ресторанів";
                return View(new PaginatedResult<AdminRestaurantListDto>());
            }
        }

        // GET: /Admin/Restaurants/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var restaurant = await _adminService.GetRestaurantDetailsAsync(id);
                if (restaurant == null)
                {
                    TempData["ErrorMessage"] = "Ресторан не знайдено";
                    return RedirectToAction(nameof(Index));
                }
                return View(restaurant);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading restaurant details: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження даних ресторану";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/Restaurants/Pending
        public async Task<IActionResult> Pending()
        {
            try
            {
                var filter = new RestaurantFilterDto
                {
                    IsApproved = false,
                    PageSize = 50
                };
                var restaurants = await _adminService.GetRestaurantsAsync(filter);
                return View(restaurants);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading pending restaurants: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження списку ресторанів на модерації";
                return View(new PaginatedResult<AdminRestaurantListDto>());
            }
        }

        // POST: /Admin/Restaurants/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid restaurantId, string? returnUrl = null)
        {
            try
            {
                var result = await _adminService.ApproveRestaurantAsync(restaurantId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Ресторан успішно схвалено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка схвалення ресторану";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving restaurant: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка схвалення ресторану";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Details), new { id = restaurantId });
        }

        // POST: /Admin/Restaurants/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid restaurantId, string? returnUrl = null)
        {
            try
            {
                var result = await _adminService.RejectRestaurantAsync(restaurantId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Ресторан відхилено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка відхилення ресторану";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting restaurant: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка відхилення ресторану";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Restaurants/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid restaurantId)
        {
            try
            {
                var result = await _adminService.ActivateRestaurantAsync(restaurantId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Ресторан активовано";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка активації ресторану";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error activating restaurant: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка активації ресторану";
            }

            return RedirectToAction(nameof(Details), new { id = restaurantId });
        }

        // POST: /Admin/Restaurants/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid restaurantId)
        {
            try
            {
                var result = await _adminService.DeactivateRestaurantAsync(restaurantId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Ресторан деактивовано";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка деактивації ресторану";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deactivating restaurant: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка деактивації ресторану";
            }

            return RedirectToAction(nameof(Details), new { id = restaurantId });
        }

        // POST: /Admin/Restaurants/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid restaurantId)
        {
            try
            {
                var result = await _adminService.DeleteRestaurantAsync(restaurantId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Ресторан успішно видалено";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка видалення ресторану";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting restaurant: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка видалення ресторану";
            }

            return RedirectToAction(nameof(Details), new { id = restaurantId });
        }
    }
}
