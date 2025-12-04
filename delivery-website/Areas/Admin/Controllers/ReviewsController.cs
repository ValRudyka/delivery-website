using delivery_website.Models.DTOs.Admin;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace delivery_website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IAdminService adminService, ILogger<ReviewsController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // GET: /Admin/Reviews
        public async Task<IActionResult> Index(ReviewFilterDto filter)
        {
            try
            {
                var reviews = await _adminService.GetReviewsAsync(filter);
                ViewBag.Filter = filter;
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading reviews: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження списку відгуків";
                return View(new PaginatedResult<AdminReviewListDto>());
            }
        }

        // GET: /Admin/Reviews/Pending
        public async Task<IActionResult> Pending()
        {
            try
            {
                var filter = new ReviewFilterDto
                {
                    IsApproved = false,
                    PageSize = 50
                };
                var reviews = await _adminService.GetReviewsAsync(filter);
                
                ViewBag.IsPendingView = true;
                return View("Index", reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading pending reviews: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження відгуків на модерації";
                return View("Index", new PaginatedResult<AdminReviewListDto>());
            }
        }

        // GET: /Admin/Reviews/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var review = await _adminService.GetReviewDetailsAsync(id);
                if (review == null)
                {
                    TempData["ErrorMessage"] = "Відгук не знайдено";
                    return RedirectToAction(nameof(Index));
                }
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading review details: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка завантаження даних відгуку";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Reviews/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid reviewId, string? notes, string? returnUrl)
        {
            try
            {
                var moderatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (moderatorId == null)
                {
                    TempData["ErrorMessage"] = "Помилка авторизації";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _adminService.ApproveReviewAsync(reviewId, moderatorId, notes);
                if (result)
                {
                    TempData["SuccessMessage"] = "Відгук успішно схвалено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка схвалення відгуку";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving review: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка схвалення відгуку";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Details), new { id = reviewId });
        }

        // POST: /Admin/Reviews/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid reviewId, string? notes, string? returnUrl)
        {
            try
            {
                var moderatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (moderatorId == null)
                {
                    TempData["ErrorMessage"] = "Помилка авторизації";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _adminService.RejectReviewAsync(reviewId, moderatorId, notes);
                if (result)
                {
                    TempData["SuccessMessage"] = "Відгук відхилено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка відхилення відгуку";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting review: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка відхилення відгуку";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Reviews/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid reviewId)
        {
            try
            {
                var result = await _adminService.DeleteReviewAsync(reviewId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Відгук успішно видалено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Помилка видалення відгуку";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting review: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка видалення відгуку";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
