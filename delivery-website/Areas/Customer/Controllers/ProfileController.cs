using delivery_website.Models.DTOs.UserProfile;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace delivery_website.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserProfileService userProfileService,
            ILogger<ProfileController> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        // GET: /Customer/Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var profile = await _userProfileService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Профіль не знайдено";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading profile: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при завантаженні профілю";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: /Customer/Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var profile = await _userProfileService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Профіль не знайдено";
                    return RedirectToAction("Index");
                }

                var model = new UpdateUserProfileDto
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    PhoneNumber = profile.PhoneNumber,
                    DateOfBirth = profile.DateOfBirth,
                    PreferredLanguage = profile.PreferredLanguage
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading edit profile: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при завантаженні форми редагування";
                return RedirectToAction("Index");
            }
        }

        // POST: /Customer/Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserProfileDto model)
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

                await _userProfileService.UpdateUserProfileAsync(userId, model);

                TempData["SuccessMessage"] = "Профіль успішно оновлено!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Помилка при оновленні профілю");
                return View(model);
            }
        }
    }
}