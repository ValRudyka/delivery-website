using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using delivery_website.Services.Interfaces;
using delivery_website.ViewModels.Customer;

namespace delivery_website.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IOrderService orderService,
            ICartService cartService,
            ILogger<CheckoutController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Customer/Checkout
        [HttpGet]
        public async Task<IActionResult> Index(Guid? restaurantId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Get cart to determine restaurantId if not provided
            var cart = await _cartService.GetUserCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Ваш кошик порожній. Додайте страви перед оформленням замовлення.";
                return RedirectToAction("Index", "Restaurants", new { area = "Customer" });
            }

            var actualRestaurantId = restaurantId ?? cart.RestaurantId;

            // Parse userId to Guid for the service
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                TempData["ErrorMessage"] = "Помилка авторизації. Спробуйте увійти знову.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var model = await _orderService.PrepareCheckoutAsync(userGuid, actualRestaurantId);

            if (model.CartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Ваш кошик порожній. Додайте страви перед оформленням замовлення.";
                return RedirectToAction("Details", "Restaurants", new { area = "Customer", id = actualRestaurantId });
            }

            return View(model);
        }

        // POST: /Customer/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Validate required fields
            if (string.IsNullOrEmpty(model.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Оберіть спосіб оплати");
            }

            if (string.IsNullOrEmpty(model.ContactPhone))
            {
                ModelState.AddModelError("ContactPhone", "Вкажіть контактний телефон");
            }

            if (!model.UseExistingAddress && string.IsNullOrEmpty(model.NewStreetAddress))
            {
                if (!model.SelectedAddressId.HasValue)
                {
                    ModelState.AddModelError("", "Оберіть адресу доставки або введіть нову");
                }
            }

            if (!ModelState.IsValid)
            {
                // Reload cart data for view
                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    var reloadedModel = await _orderService.PrepareCheckoutAsync(userGuid, model.RestaurantId);
                    model.CartItems = reloadedModel.CartItems;
                    model.SavedAddresses = reloadedModel.SavedAddresses;
                    model.Subtotal = reloadedModel.Subtotal;
                    model.Tax = reloadedModel.Tax;
                    model.DeliveryFee = reloadedModel.DeliveryFee;
                    model.Total = reloadedModel.Total;
                }
                return View(model);
            }

            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                TempData["ErrorMessage"] = "Помилка авторизації.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                // Create the order
                var order = await _orderService.CreateOrderAsync(model, userIdGuid);

                _logger.LogInformation($"Order {order.OrderNumber} created for user {userId}");

                // Go directly to confirmation
                await _orderService.UpdateOrderStatusAsync(order.OrderId, "Confirmed");
                TempData["SuccessMessage"] = "Замовлення успішно оформлено!";
                return RedirectToAction("Confirmation", new { orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                ModelState.AddModelError("", "Помилка при створенні замовлення: " + ex.Message);

                // Reload cart data
                if (Guid.TryParse(userId, out Guid reloadUserGuid))
                {
                    var reloadedModel = await _orderService.PrepareCheckoutAsync(reloadUserGuid, model.RestaurantId);
                    model.CartItems = reloadedModel.CartItems;
                    model.SavedAddresses = reloadedModel.SavedAddresses;
                }
                return View(model);
            }
        }

        // GET: /Customer/Checkout/Confirmation
        [HttpGet]
        public async Task<IActionResult> Confirmation(Guid orderId)
        {
            var model = await _orderService.GetOrderConfirmationAsync(orderId);

            if (model == null)
            {
                TempData["ErrorMessage"] = "Замовлення не знайдено.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(model);
        }
    }
}