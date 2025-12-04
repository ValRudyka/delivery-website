using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace delivery_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IAdminService adminService, ILogger<DashboardController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboard = await _adminService.GetDashboardDataAsync();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження панелі керування";
                return View();
            }
        }
    }
}
