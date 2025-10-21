using delivery_website.Models.Entities;

namespace delivery_website.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetActiveCartByUserIdAsync(string userId);
        Task<Cart?> GetCartByIdAsync(Guid cartId, bool includeItems = false);
        Task<CartItem?> GetCartItemAsync(Guid cartItemId);
        Task<Cart> CreateCartAsync(Cart cart);
        Task<Cart> UpdateCartAsync(Cart cart);
        Task DeleteCartAsync(Guid cartId);
        Task<CartItem> AddCartItemAsync(CartItem cartItem);
        Task<CartItem> UpdateCartItemAsync(CartItem cartItem);
        Task DeleteCartItemAsync(Guid cartItemId);
        Task ClearCartAsync(Guid cartId);
        Task DeleteExpiredCartsAsync();
    }
}