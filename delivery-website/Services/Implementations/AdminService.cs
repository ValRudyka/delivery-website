using delivery_website.Data;
using delivery_website.Models.DTOs.Admin;
using delivery_website.Models.Entities;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace delivery_website.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<AdminService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        #region Dashboard

        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var dashboard = new AdminDashboardDto();

            // Users Statistics
            var users = await _userManager.Users.ToListAsync();
            dashboard.TotalUsers = users.Count;

            var userProfiles = await _context.UserProfiles.ToListAsync();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Customer")) dashboard.TotalCustomers++;
                if (roles.Contains("RestaurantOwner")) dashboard.TotalRestaurantOwners++;
            }

            dashboard.NewUsersToday = userProfiles.Count(u => u.CreatedDate.Date == today);
            dashboard.NewUsersThisWeek = userProfiles.Count(u => u.CreatedDate >= weekStart);
            dashboard.NewUsersThisMonth = userProfiles.Count(u => u.CreatedDate >= monthStart);

            // Restaurants Statistics
            var restaurants = await _context.Restaurants.ToListAsync();
            dashboard.TotalRestaurants = restaurants.Count;
            dashboard.ActiveRestaurants = restaurants.Count(r => r.IsActive && r.IsApproved);
            dashboard.PendingApprovalRestaurants = restaurants.Count(r => !r.IsApproved);
            dashboard.InactiveRestaurants = restaurants.Count(r => !r.IsActive);

            // Orders Statistics
            var orders = await _context.Orders.ToListAsync();
            dashboard.TotalOrders = orders.Count;
            dashboard.OrdersToday = orders.Count(o => o.OrderDate.Date == today);
            dashboard.OrdersThisWeek = orders.Count(o => o.OrderDate >= weekStart);
            dashboard.OrdersThisMonth = orders.Count(o => o.OrderDate >= monthStart);
            dashboard.PendingOrders = orders.Count(o => o.OrderStatus == "Pending");
            dashboard.ProcessingOrders = orders.Count(o => o.OrderStatus == "Preparing" || o.OrderStatus == "Confirmed");
            dashboard.CompletedOrdersToday = orders.Count(o => o.OrderStatus == "Delivered" && o.DeliveredDate?.Date == today);
            dashboard.CancelledOrdersToday = orders.Count(o => o.OrderStatus == "Cancelled" && o.CancelledDate?.Date == today);

            // Revenue Statistics
            var completedOrders = orders.Where(o => o.OrderStatus == "Delivered").ToList();
            dashboard.TotalRevenue = completedOrders.Sum(o => o.TotalAmount);
            dashboard.RevenueToday = completedOrders.Where(o => o.DeliveredDate?.Date == today).Sum(o => o.TotalAmount);
            dashboard.RevenueThisWeek = completedOrders.Where(o => o.DeliveredDate >= weekStart).Sum(o => o.TotalAmount);
            dashboard.RevenueThisMonth = completedOrders.Where(o => o.DeliveredDate >= monthStart).Sum(o => o.TotalAmount);

            // Reviews Statistics
            var reviews = await _context.Reviews.ToListAsync();
            dashboard.TotalReviews = reviews.Count;
            dashboard.PendingReviews = reviews.Count(r => !r.IsApproved);
            dashboard.ApprovedReviews = reviews.Count(r => r.IsApproved);
            dashboard.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

            // Recent Activity
            dashboard.RecentOrders = await _context.Orders
                .Include(o => o.Restaurant)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.UserId,
                    RestaurantName = o.Restaurant.Name,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            // Fill in customer names
            foreach (var order in dashboard.RecentOrders)
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == order.CustomerName);
                order.CustomerName = profile?.FullName ?? "Unknown";
            }

            dashboard.RecentUsers = userProfiles
                .OrderByDescending(u => u.CreatedDate)
                .Take(5)
                .Select(u => new RecentUserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = "",
                    Role = "Customer",
                    CreatedDate = u.CreatedDate
                })
                .ToList();

            dashboard.RecentReviews = await _context.Reviews
                .Include(r => r.Restaurant)
                .OrderByDescending(r => r.CreatedDate)
                .Take(5)
                .Select(r => new RecentReviewDto
                {
                    ReviewId = r.ReviewId,
                    CustomerName = r.UserId,
                    RestaurantName = r.Restaurant.Name,
                    Rating = r.Rating,
                    IsApproved = r.IsApproved,
                    CreatedDate = r.CreatedDate
                })
                .ToListAsync();

            return dashboard;
        }

        #endregion

        #region User Management

        public async Task<PaginatedResult<UserListDto>> GetUsersAsync(UserFilterDto filter)
        {
            var users = _userManager.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                users = users.Where(u => u.Email!.ToLower().Contains(searchLower) ||
                                        u.UserName!.ToLower().Contains(searchLower));
            }

            // Lock status filter
            if (filter.IsLockedOut.HasValue)
            {
                if (filter.IsLockedOut.Value)
                    users = users.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                else
                    users = users.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
            }

            var totalCount = await users.CountAsync();

            var userList = await users
                .OrderByDescending(u => u.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<UserListDto>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Customer";

                // Role filter (applied after fetching due to Identity limitations)
                if (!string.IsNullOrWhiteSpace(filter.Role) && role != filter.Role)
                    continue;

                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                var ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

                result.Add(new UserListDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = profile?.FullName ?? "Unknown",
                    PhoneNumber = profile?.PhoneNumber ?? "",
                    Role = role,
                    IsEmailConfirmed = user.EmailConfirmed,
                    IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd?.DateTime,
                    CreatedDate = profile?.CreatedDate ?? DateTime.UtcNow,
                    OrdersCount = ordersCount
                });
            }

            return new PaginatedResult<UserListDto>(result, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<UserDetailsDto?> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
            var reviewsCount = await _context.Reviews.CountAsync(r => r.UserId == userId);
            var addressesCount = await _context.Addresses.CountAsync(a => a.UserId == userId);

            return new UserDetailsDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = profile?.FirstName ?? "",
                LastName = profile?.LastName ?? "",
                PhoneNumber = profile?.PhoneNumber ?? "",
                DateOfBirth = profile?.DateOfBirth,
                ProfileImageUrl = profile?.ProfileImageUrl,
                PreferredLanguage = profile?.PreferredLanguage ?? "uk",
                Role = role,
                IsEmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd?.DateTime,
                CreatedDate = profile?.CreatedDate ?? DateTime.UtcNow,
                OrdersCount = orders.Count,
                TotalSpent = orders.Where(o => o.OrderStatus == "Delivered").Sum(o => o.TotalAmount),
                ReviewsCount = reviewsCount,
                AddressesCount = addressesCount
            };
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                _logger.LogInformation($"User {userId} role changed to {newRole}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user role: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LockUserAsync(string userId, int? durationInDays = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var lockoutEnd = durationInDays.HasValue
                    ? DateTimeOffset.UtcNow.AddDays(durationInDays.Value)
                    : DateTimeOffset.MaxValue;

                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                _logger.LogInformation($"User {userId} locked until {lockoutEnd}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error locking user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                _logger.LogInformation($"User {userId} unlocked");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unlocking user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                // Delete related data
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null) _context.UserProfiles.Remove(profile);

                var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
                _context.Addresses.RemoveRange(addresses);

                var carts = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
                _context.Carts.RemoveRange(carts);

                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);

                _logger.LogInformation($"User {userId} deleted");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Restaurant Management

        public async Task<PaginatedResult<AdminRestaurantListDto>> GetRestaurantsAsync(RestaurantFilterDto filter)
        {
            var query = _context.Restaurants.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(searchLower) ||
                                        r.City.ToLower().Contains(searchLower) ||
                                        r.Email.ToLower().Contains(searchLower));
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(filter.CuisineType))
                query = query.Where(r => r.CuisineType == filter.CuisineType);

            if (filter.IsActive.HasValue)
                query = query.Where(r => r.IsActive == filter.IsActive.Value);

            if (filter.IsApproved.HasValue)
                query = query.Where(r => r.IsApproved == filter.IsApproved.Value);

            // Sorting
            query = filter.SortBy switch
            {
                "Name" => filter.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                "Rating" => filter.SortDescending ? query.OrderByDescending(r => r.AverageRating) : query.OrderBy(r => r.AverageRating),
                _ => filter.SortDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var restaurants = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<AdminRestaurantListDto>();

            foreach (var restaurant in restaurants)
            {
                var owner = await _userManager.FindByIdAsync(restaurant.OwnerId);
                var orders = await _context.Orders.Where(o => o.RestaurantId == restaurant.RestaurantId).ToListAsync();

                result.Add(new AdminRestaurantListDto
                {
                    RestaurantId = restaurant.RestaurantId,
                    Name = restaurant.Name,
                    CuisineType = restaurant.CuisineType,
                    OwnerEmail = owner?.Email ?? "Unknown",
                    OwnerId = restaurant.OwnerId,
                    City = restaurant.City,
                    IsActive = restaurant.IsActive,
                    IsApproved = restaurant.IsApproved,
                    AverageRating = restaurant.AverageRating,
                    TotalReviews = restaurant.TotalReviews,
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Where(o => o.OrderStatus == "Delivered").Sum(o => o.TotalAmount),
                    CreatedDate = restaurant.CreatedDate,
                    ApprovedDate = restaurant.ApprovedDate
                });
            }

            return new PaginatedResult<AdminRestaurantListDto>(result, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<AdminRestaurantDetailsDto?> GetRestaurantDetailsAsync(Guid restaurantId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Categories)
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);

            if (restaurant == null) return null;

            var owner = await _userManager.FindByIdAsync(restaurant.OwnerId);
            var ownerProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == restaurant.OwnerId);
            var orders = await _context.Orders.Where(o => o.RestaurantId == restaurantId).ToListAsync();

            return new AdminRestaurantDetailsDto
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Description = restaurant.Description,
                CuisineType = restaurant.CuisineType,
                PhoneNumber = restaurant.PhoneNumber,
                Email = restaurant.Email,
                FullAddress = restaurant.FullAddress,
                LogoUrl = restaurant.LogoUrl,
                CoverImageUrl = restaurant.CoverImageUrl,
                WebsiteUrl = restaurant.WebsiteUrl,
                MinimumOrderAmount = restaurant.MinimumOrderAmount,
                DeliveryFee = restaurant.DeliveryFee,
                EstimatedDeliveryTime = restaurant.EstimatedDeliveryTime,
                OpeningHours = restaurant.OpeningHours,
                IsActive = restaurant.IsActive,
                IsApproved = restaurant.IsApproved,
                AverageRating = restaurant.AverageRating,
                TotalReviews = restaurant.TotalReviews,
                CreatedDate = restaurant.CreatedDate,
                ApprovedDate = restaurant.ApprovedDate,
                OwnerId = restaurant.OwnerId,
                OwnerEmail = owner?.Email ?? "Unknown",
                OwnerName = ownerProfile?.FullName ?? "Unknown",
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == "Pending"),
                CompletedOrders = orders.Count(o => o.OrderStatus == "Delivered"),
                CancelledOrders = orders.Count(o => o.OrderStatus == "Cancelled"),
                TotalRevenue = orders.Where(o => o.OrderStatus == "Delivered").Sum(o => o.TotalAmount),
                MenuItemsCount = restaurant.MenuItems.Count,
                CategoriesCount = restaurant.Categories.Count
            };
        }

        public async Task<bool> ApproveRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var restaurant = await _context.Restaurants.FindAsync(restaurantId);
                if (restaurant == null) return false;

                restaurant.IsApproved = true;
                restaurant.ApprovedDate = DateTime.UtcNow;
                restaurant.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Restaurant {restaurantId} approved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving restaurant: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RejectRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var restaurant = await _context.Restaurants.FindAsync(restaurantId);
                if (restaurant == null) return false;

                restaurant.IsApproved = false;
                restaurant.IsActive = false;
                restaurant.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Restaurant {restaurantId} rejected");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting restaurant: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActivateRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var restaurant = await _context.Restaurants.FindAsync(restaurantId);
                if (restaurant == null) return false;

                restaurant.IsActive = true;
                restaurant.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Restaurant {restaurantId} activated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error activating restaurant: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeactivateRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var restaurant = await _context.Restaurants.FindAsync(restaurantId);
                if (restaurant == null) return false;

                restaurant.IsActive = false;
                restaurant.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Restaurant {restaurantId} deactivated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deactivating restaurant: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var restaurant = await _context.Restaurants
                    .Include(r => r.Categories)
                    .Include(r => r.MenuItems)
                    .Include(r => r.Reviews)
                    .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);

                if (restaurant == null) return false;

                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Restaurant {restaurantId} deleted");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting restaurant: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Order Management

        public async Task<PaginatedResult<AdminOrderListDto>> GetOrdersAsync(OrderFilterDto filter)
        {
            var query = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(o => o.OrderNumber.ToLower().Contains(searchLower) ||
                                        o.Restaurant.Name.ToLower().Contains(searchLower));
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(filter.OrderStatus))
                query = query.Where(o => o.OrderStatus == filter.OrderStatus);

            if (filter.RestaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == filter.RestaurantId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= filter.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
                query = query.Where(o => o.Payment != null && o.Payment.PaymentStatus == filter.PaymentStatus);

            // Sorting
            query = filter.SortBy switch
            {
                "OrderNumber" => filter.SortDescending ? query.OrderByDescending(o => o.OrderNumber) : query.OrderBy(o => o.OrderNumber),
                "TotalAmount" => filter.SortDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                "Status" => filter.SortDescending ? query.OrderByDescending(o => o.OrderStatus) : query.OrderBy(o => o.OrderStatus),
                _ => filter.SortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate)
            };

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<AdminOrderListDto>();

            foreach (var order in orders)
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == order.UserId);
                var user = await _userManager.FindByIdAsync(order.UserId);

                result.Add(new AdminOrderListDto
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    CustomerName = profile?.FullName ?? "Unknown",
                    CustomerEmail = user?.Email ?? "Unknown",
                    RestaurantName = order.Restaurant.Name,
                    RestaurantId = order.RestaurantId,
                    OrderStatus = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.Payment?.PaymentMethod,
                    PaymentStatus = order.Payment?.PaymentStatus,
                    OrderDate = order.OrderDate,
                    DeliveredDate = order.DeliveredDate,
                    ItemsCount = order.OrderItems.Sum(i => i.Quantity)
                });
            }

            return new PaginatedResult<AdminOrderListDto>(result, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<AdminOrderDetailsDto?> GetOrderDetailsAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .Include(o => o.DeliveryAddress)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == order.UserId);
            var user = await _userManager.FindByIdAsync(order.UserId);

            return new AdminOrderDetailsDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                CustomerId = order.UserId,
                CustomerName = profile?.FullName ?? "Unknown",
                CustomerEmail = user?.Email ?? "Unknown",
                CustomerPhone = profile?.PhoneNumber ?? "",
                RestaurantId = order.RestaurantId,
                RestaurantName = order.Restaurant.Name,
                RestaurantPhone = order.Restaurant.PhoneNumber,
                DeliveryAddress = order.DeliveryAddress?.FullAddress,
                DeliveryInstructions = order.DeliveryInstructions,
                SubtotalAmount = order.SubtotalAmount,
                TaxAmount = order.TaxAmount,
                DeliveryFee = order.DeliveryFee,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                ConfirmedDate = order.ConfirmedDate,
                PreparingDate = order.PreparingDate,
                ReadyDate = order.ReadyDate,
                OutForDeliveryDate = order.OutForDeliveryDate,
                DeliveredDate = order.DeliveredDate,
                CancelledDate = order.CancelledDate,
                EstimatedDeliveryTime = order.EstimatedDeliveryTime,
                CancellationReason = order.CancellationReason,
                PaymentMethod = order.Payment?.PaymentMethod,
                PaymentStatus = order.Payment?.PaymentStatus,
                TransactionId = order.Payment?.TransactionId,
                Items = order.OrderItems.Select(oi => new AdminOrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    MenuItemName = oi.MenuItemName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Customizations = oi.Customizations
                }).ToList()
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? cancellationReason = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                order.OrderStatus = newStatus;
                order.UpdatedDate = DateTime.UtcNow;

                switch (newStatus)
                {
                    case "Confirmed":
                        order.ConfirmedDate = DateTime.UtcNow;
                        break;
                    case "Preparing":
                        order.PreparingDate = DateTime.UtcNow;
                        break;
                    case "Ready":
                        order.ReadyDate = DateTime.UtcNow;
                        break;
                    case "OutForDelivery":
                        order.OutForDeliveryDate = DateTime.UtcNow;
                        break;
                    case "Delivered":
                        order.DeliveredDate = DateTime.UtcNow;
                        break;
                    case "Cancelled":
                        order.CancelledDate = DateTime.UtcNow;
                        order.CancellationReason = cancellationReason;
                        break;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order {orderId} status updated to {newStatus}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order status: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Review Management

        public async Task<PaginatedResult<AdminReviewListDto>> GetReviewsAsync(ReviewFilterDto filter)
        {
            var query = _context.Reviews
                .Include(r => r.Restaurant)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(r => r.ReviewText.ToLower().Contains(searchLower) ||
                                        r.Restaurant.Name.ToLower().Contains(searchLower));
            }

            // Filters
            if (filter.RestaurantId.HasValue)
                query = query.Where(r => r.RestaurantId == filter.RestaurantId.Value);

            if (filter.MinRating.HasValue)
                query = query.Where(r => r.Rating >= filter.MinRating.Value);

            if (filter.MaxRating.HasValue)
                query = query.Where(r => r.Rating <= filter.MaxRating.Value);

            if (filter.IsApproved.HasValue)
                query = query.Where(r => r.IsApproved == filter.IsApproved.Value);

            if (filter.IsVerifiedPurchase.HasValue)
                query = query.Where(r => r.IsVerifiedPurchase == filter.IsVerifiedPurchase.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.CreatedDate <= filter.ToDate.Value);

            // Sorting
            query = filter.SortBy switch
            {
                "Rating" => filter.SortDescending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
                "Restaurant" => filter.SortDescending ? query.OrderByDescending(r => r.Restaurant.Name) : query.OrderBy(r => r.Restaurant.Name),
                _ => filter.SortDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<AdminReviewListDto>();

            foreach (var review in reviews)
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == review.UserId);
                var user = await _userManager.FindByIdAsync(review.UserId);
                var moderator = review.ModeratorId != null
                    ? await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == review.ModeratorId)
                    : null;

                result.Add(new AdminReviewListDto
                {
                    ReviewId = review.ReviewId,
                    CustomerName = profile?.FullName ?? "Unknown",
                    CustomerEmail = user?.Email ?? "Unknown",
                    RestaurantName = review.Restaurant.Name,
                    RestaurantId = review.RestaurantId,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    IsVerifiedPurchase = review.IsVerifiedPurchase,
                    IsApproved = review.IsApproved,
                    CreatedDate = review.CreatedDate,
                    ModerationDate = review.ModerationDate,
                    ModeratorName = moderator?.FullName
                });
            }

            return new PaginatedResult<AdminReviewListDto>(result, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<AdminReviewDetailsDto?> GetReviewDetailsAsync(Guid reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Restaurant)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null) return null;

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == review.UserId);
            var user = await _userManager.FindByIdAsync(review.UserId);
            var moderator = review.ModeratorId != null
                ? await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == review.ModeratorId)
                : null;

            return new AdminReviewDetailsDto
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                CustomerName = profile?.FullName ?? "Unknown",
                CustomerEmail = user?.Email ?? "Unknown",
                RestaurantId = review.RestaurantId,
                RestaurantName = review.Restaurant.Name,
                OrderId = review.OrderId,
                OrderNumber = review.Order?.OrderNumber,
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                ImageUrls = !string.IsNullOrEmpty(review.ImageUrls)
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(review.ImageUrls)
                    : null,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                IsApproved = review.IsApproved,
                ModerationDate = review.ModerationDate,
                ModeratorId = review.ModeratorId,
                ModeratorName = moderator?.FullName,
                ModerationNotes = review.ModerationNotes,
                CreatedDate = review.CreatedDate
            };
        }

        public async Task<bool> ApproveReviewAsync(Guid reviewId, string moderatorId, string? notes = null)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                review.IsApproved = true;
                review.ModerationDate = DateTime.UtcNow;
                review.ModeratorId = moderatorId;
                review.ModerationNotes = notes;
                review.UpdatedDate = DateTime.UtcNow;

                // Update restaurant rating
                await UpdateRestaurantRatingAsync(review.RestaurantId);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Review {reviewId} approved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving review: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RejectReviewAsync(Guid reviewId, string moderatorId, string? notes = null)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                review.IsApproved = false;
                review.ModerationDate = DateTime.UtcNow;
                review.ModeratorId = moderatorId;
                review.ModerationNotes = notes;
                review.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Review {reviewId} rejected");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting review: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                var restaurantId = review.RestaurantId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                // Update restaurant rating
                await UpdateRestaurantRatingAsync(restaurantId);

                _logger.LogInformation($"Review {reviewId} deleted");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting review: {ex.Message}");
                return false;
            }
        }

        private async Task UpdateRestaurantRatingAsync(Guid restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null) return;

            var approvedReviews = await _context.Reviews
                .Where(r => r.RestaurantId == restaurantId && r.IsApproved)
                .ToListAsync();

            if (approvedReviews.Any())
            {
                restaurant.AverageRating = (decimal)approvedReviews.Average(r => r.Rating);
                restaurant.TotalReviews = approvedReviews.Count;
            }
            else
            {
                restaurant.AverageRating = 0;
                restaurant.TotalReviews = 0;
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
