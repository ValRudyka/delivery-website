using System;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public CheckoutController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /Customer/Checkout
        [HttpGet]
        public async Task<IActionResult> Index(Guid restaurantId)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            // Prepare checkout data
            var model = await _orderService.PrepareCheckoutAsync(userId, restaurantId);

            if (model.CartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToAction("Details", "Restaurant", new { id = restaurantId });
            }

            return View(model);
        }

        // POST: /Customer/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();

            try
            {
                // Create the order
                var order = await _orderService.CreateOrderAsync(model, userId);

                // If Cash on Delivery, go directly to confirmation
                if (model.PaymentMethod == "CashOnDelivery")
                {
                    await _orderService.UpdateOrderStatusAsync(order.OrderId, "Confirmed");
                    return RedirectToAction("Confirmation", new { orderId = order.OrderId });
                }

                // If Credit Card, redirect to Stripe Checkout
                if (model.PaymentMethod == "CreditCard")
                {
                    try
                    {
                        var sessionId = await _orderService.CreateStripeCheckoutSessionAsync(order);

                        // Store session ID in TempData for later verification
                        TempData["StripeSessionId"] = sessionId;

                        // Redirect to Stripe Checkout
                        return Redirect($"https://checkout.stripe.com/pay/{sessionId}");
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Payment processing error: " + ex.Message;
                        return RedirectToAction("Cancel", new { orderId = order.OrderId });
                    }
                }

                return RedirectToAction("Confirmation", new { orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your order: " + ex.Message);
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
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: /Customer/Checkout/Success (Stripe callback)
        [HttpGet]
        public async Task<IActionResult> Success(string session_id, Guid orderId)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                TempData["Error"] = "Invalid payment session.";
                return RedirectToAction("Cancel", new { orderId });
            }

            // Process the successful payment
            var success = await _orderService.ProcessPaymentSuccessAsync(session_id, orderId);

            if (success)
            {
                TempData["Success"] = "Payment successful! Your order has been confirmed.";
                return RedirectToAction("Confirmation", new { orderId });
            }
            else
            {
                TempData["Error"] = "Payment verification failed. Please contact support.";
                return RedirectToAction("Cancel", new { orderId });
            }
        }

        // GET: /Customer/Checkout/Cancel (Stripe callback)
        [HttpGet]
        public async Task<IActionResult> Cancel(Guid orderId)
        {
            // Update order status to cancelled
            await _orderService.UpdateOrderStatusAsync(orderId, "Cancelled");

            TempData["Warning"] = "Payment was cancelled. Your order has been cancelled.";

            return View(orderId);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }

            return Guid.Empty;
        }
    }
}