using delivery_website.Data;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace delivery_website.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetActiveCartByUserIdAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Restaurant)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<Cart?> GetCartByIdAsync(Guid cartId, bool includeItems = false)
        {
            var query = _context.Carts.AsQueryable();

            if (includeItems)
            {
                query = query
                    .Include(c => c.Restaurant)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.MenuItem);
            }

            return await query.FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task<CartItem?> GetCartItemAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.MenuItem)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            cart.CreatedDate = DateTime.UtcNow;
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            cart.UpdatedDate = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task DeleteCartAsync(Guid cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CartItem> AddCartItemAsync(CartItem cartItem)
        {
            cartItem.CreatedDate = DateTime.UtcNow;
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<CartItem> UpdateCartItemAsync(CartItem cartItem)
        {
            cartItem.UpdatedDate = DateTime.UtcNow;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task DeleteCartItemAsync(Guid cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(Guid cartId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredCartsAsync()
        {
            var expiredCarts = await _context.Carts
                .Where(c => c.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.Carts.RemoveRange(expiredCarts);
            await _context.SaveChangesAsync();
        }
    }
}