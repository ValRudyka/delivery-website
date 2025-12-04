using delivery_website.Models.DTOs.Admin;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace delivery_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IAdminService adminService, ILogger<OrdersController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Index(OrderFilterDto filter)
        {
            try
            {
                var orders = await _adminService.GetOrdersAsync(filter);
                ViewBag.Filter = filter;
                ViewBag.Statuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "OutForDelivery", "Delivered", "Cancelled" };
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading orders: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження списку замовлень";
                return View(new PaginatedResult<AdminOrderListDto>());
            }
        }

        // GET: /Admin/Orders/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var order = await _adminService.GetOrderDetailsAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Замовлення не знайдено";
                    return RedirectToAction(nameof(Index));
                }
                
                ViewBag.Statuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "OutForDelivery", "Delivered", "Cancelled" };
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading order details: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження даних замовлення";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid orderId, string newStatus, string? cancellationReason)
        {
            try
            {
                var result = await _adminService.UpdateOrderStatusAsync(orderId, newStatus, cancellationReason);
                if (result)
                {
                    TempData["SuccessMessage"] = "Статус замовлення успішно оновлено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка оновлення статусу замовлення";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order status: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка оновлення статусу замовлення";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // GET: /Admin/Orders/ByStatus/{status}
        public async Task<IActionResult> ByStatus(string status)
        {
            try
            {
                var filter = new OrderFilterDto
                {
                    OrderStatus = status,
                    PageSize = 50
                };
                var orders = await _adminService.GetOrdersAsync(filter);
                
                ViewBag.StatusFilter = status;
                ViewBag.Statuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "OutForDelivery", "Delivered", "Cancelled" };
                
                return View("Index", orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading orders by status: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження замовлень";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
