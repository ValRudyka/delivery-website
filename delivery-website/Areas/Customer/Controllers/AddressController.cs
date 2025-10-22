using delivery_website.Models.DTOs.Address;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace delivery_website.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(
            IAddressService addressService,
            ILogger<AddressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        // GET: /Customer/Address
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var addresses = await _addressService.GetUserAddressesAsync(userId);
                return View(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading addresses: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при завантаженні адрес";
                return RedirectToAction("Index", "Profile");
            }
        }

        // GET: /Customer/Address/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Address/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAddressDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _addressService.CreateAddressAsync(userId, model);

                TempData["SuccessMessage"] = "Адресу успішно додано!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating address: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Помилка при додаванні адреси");
                return View(model);
            }
        }

        // GET: /Customer/Address/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var address = await _addressService.GetAddressByIdAsync(userId, id);
                if (address == null)
                {
                    TempData["ErrorMessage"] = "Адресу не знайдено";
                    return RedirectToAction("Index");
                }

                var model = new UpdateAddressDto
                {
                    AddressId = address.AddressId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                    Label = address.Label
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading address for edit: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при завантаженні адреси";
                return RedirectToAction("Index");
            }
        }

        // POST: /Customer/Address/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateAddressDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await _addressService.UpdateAddressAsync(userId, model);

                TempData["SuccessMessage"] = "Адресу успішно оновлено!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating address: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // POST: /Customer/Address/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { success = false, message = "Необхідно увійти в систему" });
                }

                await _addressService.DeleteAddressAsync(userId, id);

                TempData["SuccessMessage"] = "Адресу успішно видалено!";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting address: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Customer/Address/SetDefault/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Json(new { success = false, message = "Необхідно увійти в систему" });
                }

                await _addressService.SetDefaultAddressAsync(userId, id);

                TempData["SuccessMessage"] = "Адреса за замовчуванням встановлена!";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting default address: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}