using delivery_website.Models.DTOs.Cart;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using delivery_website.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace delivery_website.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IRestaurantRepository restaurantRepository,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _restaurantRepository = restaurantRepository;
            _logger = logger;
        }

        public async Task<CartDto?> GetUserCartAsync(string userId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart == null)
                return null;

            return MapToCartDto(cart);
        }

        public async Task<CartDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    RestaurantId = dto.RestaurantId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                cart = await _cartRepository.CreateCartAsync(cart);

                cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            }
            else if (cart.RestaurantId != dto.RestaurantId)
            {
                // Different restaurant - clear cart and set new restaurant
                await _cartRepository.ClearCartAsync(cart.CartId);
                cart.RestaurantId = dto.RestaurantId;
                await _cartRepository.UpdateCartAsync(cart);

                cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            }

            // Check if item already exists in cart
            var existingItem = cart!.CartItems
                .FirstOrDefault(ci => ci.MenuItemId == dto.MenuItemId &&
                                     ci.Customizations == dto.Customizations);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(dto.RestaurantId, includeMenuItems: true);
                var menuItem = restaurant?.MenuItems.FirstOrDefault(m => m.MenuItemId == dto.MenuItemId);

                if (menuItem == null)
                    throw new Exception("Menu item not found");

                var cartItem = new CartItem
                {
                    CartItemId = Guid.NewGuid(),
                    CartId = cart.CartId,
                    MenuItemId = dto.MenuItemId,
                    Quantity = dto.Quantity,
                    UnitPrice = menuItem.Price,
                    Customizations = dto.Customizations
                };

                await _cartRepository.AddCartItemAsync(cartItem);
            }

            // Reload cart with updated items
            cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            return MapToCartDto(cart!);
        }

        public async Task<CartDto> UpdateCartItemQuantityAsync(string userId, UpdateCartItemDto dto)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart == null)
                throw new Exception("Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == dto.CartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            if (dto.Quantity <= 0)
            {
                await _cartRepository.DeleteCartItemAsync(dto.CartItemId);
            }
            else
            {
                cartItem.Quantity = dto.Quantity;
                await _cartRepository.UpdateCartItemAsync(cartItem);
            }

            cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            return MapToCartDto(cart!);
        }

        public async Task<CartDto> RemoveCartItemAsync(string userId, Guid cartItemId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart == null)
                throw new Exception("Cart not found");

            await _cartRepository.DeleteCartItemAsync(cartItemId);

            cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart == null || !cart.CartItems.Any())
            {
                // Cart is empty, delete it
                if (cart != null)
                    await _cartRepository.DeleteCartAsync(cart.CartId);

                return new CartDto
                {
                    Items = new List<CartItemDto>()
                };
            }

            return MapToCartDto(cart);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart != null)
            {
                await _cartRepository.DeleteCartAsync(cart.CartId);
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }

        private CartDto MapToCartDto(Cart cart)
        {
            var subtotal = cart.CartItems.Sum(ci => ci.TotalPrice);
            var deliveryFee = cart.Restaurant.DeliveryFee ?? 0;

            return new CartDto
            {
                CartId = cart.CartId,
                RestaurantId = cart.RestaurantId,
                RestaurantName = cart.Restaurant.Name,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    MenuItemId = ci.MenuItemId,
                    MenuItemName = ci.MenuItem.Name,
                    MenuItemImage = ci.MenuItem.ImageUrl,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.TotalPrice,
                    Customizations = ci.Customizations
                }).ToList(),
                SubtotalAmount = subtotal,
                DeliveryFee = deliveryFee,
                TotalAmount = subtotal + deliveryFee
            };
        }
    }
}