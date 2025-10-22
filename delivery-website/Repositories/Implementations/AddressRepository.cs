using delivery_website.Data;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace delivery_website.Repositories.Implementations
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public AddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetUserAddressesAsync(string userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(Guid addressId)
        {
            return await _context.Addresses.FindAsync(addressId);
        }

        public async Task<Address?> GetDefaultAddressAsync(string userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
        }

        public async Task<Address> CreateAsync(Address address)
        {
            address.CreatedDate = DateTime.UtcNow;

            // If this is set as default, unset other defaults
            if (address.IsDefault)
            {
                await UnsetAllDefaultsAsync(address.UserId);
            }

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAsync(Address address)
        {
            address.UpdatedDate = DateTime.UtcNow;

            // If this is set as default, unset other defaults
            if (address.IsDefault)
            {
                await UnsetAllDefaultsAsync(address.UserId, address.AddressId);
            }

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task DeleteAsync(Guid addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetDefaultAddressAsync(string userId, Guid addressId)
        {
            // Unset all defaults first
            await UnsetAllDefaultsAsync(userId);

            // Set the new default
            var address = await _context.Addresses.FindAsync(addressId);
            if (address != null && address.UserId == userId)
            {
                address.IsDefault = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserOwnsAddressAsync(string userId, Guid addressId)
        {
            return await _context.Addresses
                .AnyAsync(a => a.AddressId == addressId && a.UserId == userId);
        }

        private async Task UnsetAllDefaultsAsync(string userId, Guid? exceptAddressId = null)
        {
            var query = _context.Addresses.Where(a => a.UserId == userId && a.IsDefault);

            if (exceptAddressId.HasValue)
            {
                query = query.Where(a => a.AddressId != exceptAddressId.Value);
            }

            var addresses = await query.ToListAsync();
            foreach (var address in addresses)
            {
                address.IsDefault = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}