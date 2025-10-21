using delivery_website.Models.DTOs.Cart;

namespace delivery_website.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto?> GetUserCartAsync(string userId);
        Task<CartDto> AddToCartAsync(string userId, AddToCartDto dto);
        Task<CartDto> UpdateCartItemQuantityAsync(string userId, UpdateCartItemDto dto);
        Task<CartDto> RemoveCartItemAsync(string userId, Guid cartItemId);
        Task ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }
}