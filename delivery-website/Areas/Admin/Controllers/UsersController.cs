using delivery_website.Models.DTOs.Admin;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace delivery_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAdminService adminService, ILogger<UsersController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(UserFilterDto filter)
        {
            try
            {
                var users = await _adminService.GetUsersAsync(filter);
                ViewBag.Filter = filter;
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading users: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження списку користувачів";
                return View(new PaginatedResult<UserListDto>());
            }
        }

        // GET: /Admin/Users/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var user = await _adminService.GetUserDetailsAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading user details: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження даних користувача";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Users/UpdateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            try
            {
                var result = await _adminService.UpdateUserRoleAsync(userId, newRole);
                if (result)
                {
                    TempData["SuccessMessage"] = "Роль користувача успішно оновлено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка оновлення ролі користувача";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user role: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка оновлення ролі користувача";
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }

        // POST: /Admin/Users/Lock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string userId, int? durationInDays)
        {
            try
            {
                var result = await _adminService.LockUserAsync(userId, durationInDays);
                if (result)
                {
                    TempData["SuccessMessage"] = "Користувача успішно заблоковано";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка блокування користувача";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error locking user: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка блокування користувача";
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }

        // POST: /Admin/Users/Unlock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string userId)
        {
            try
            {
                var result = await _adminService.UnlockUserAsync(userId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Користувача успішно розблоковано";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка розблокування користувача";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unlocking user: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка розблокування користувача";
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }

        // POST: /Admin/Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(userId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Користувача успішно видалено";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка видалення користувача";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка видалення користувача";
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}
