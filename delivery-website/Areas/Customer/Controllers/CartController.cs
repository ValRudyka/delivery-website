using delivery_website.Models.DTOs.Cart;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace delivery_website.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Customer/Cart
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var cart = await _cartService.GetUserCartAsync(userId);
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading cart: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при завантаженні кошика";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // POST: /Customer/Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { success = false, message = "Необхідно увійти в систему" });
                }

                var cart = await _cartService.AddToCartAsync(userId, dto);

                return Json(new
                {
                    success = true,
                    message = "Товар додано до кошика",
                    itemCount = cart.ItemCount,
                    cartTotal = cart.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to cart: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Customer/Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { success = false, message = "Необхідно увійти в систему" });
                }

                var cart = await _cartService.UpdateCartItemQuantityAsync(userId, dto);

                return Json(new
                {
                    success = true,
                    message = "Кількість оновлено",
                    itemCount = cart.ItemCount,
                    subtotal = cart.SubtotalAmount,
                    deliveryFee = cart.DeliveryFee,
                    total = cart.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating quantity: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Customer/Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(Guid cartItemId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { success = false, message = "Необхідно увійти в систему" });
                }

                var cart = await _cartService.RemoveCartItemAsync(userId, cartItemId);

                return Json(new
                {
                    success = true,
                    message = "Товар видалено з кошика",
                    itemCount = cart.ItemCount,
                    subtotal = cart.SubtotalAmount,
                    deliveryFee = cart.DeliveryFee,
                    total = cart.TotalAmount,
                    isEmpty = !cart.Items.Any()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing item: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Customer/Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { success = false, message = "Необхідно увійти в систему" });
                }

                await _cartService.ClearCartAsync(userId);

                TempData["SuccessMessage"] = "Кошик очищено";
                return Json(new { success = true, message = "Кошик очищено" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cart: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Customer/Cart/GetItemCount - For navbar badge
        [HttpGet]
        public async Task<IActionResult> GetItemCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { count = 0 });
                }

                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart count: {ex.Message}");
                return Json(new { count = 0 });
            }
        }
    }
}