using delivery_website.Models.Entities;

namespace delivery_website.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetUserAddressesAsync(string userId);
        Task<Address?> GetByIdAsync(Guid addressId);
        Task<Address?> GetDefaultAddressAsync(string userId);
        Task<Address> CreateAsync(Address address);
        Task<Address> UpdateAsync(Address address);
        Task DeleteAsync(Guid addressId);
        Task SetDefaultAddressAsync(string userId, Guid addressId);
        Task<bool> UserOwnsAddressAsync(string userId, Guid addressId);
    }
}