using delivery_website.Models.DTOs.Address;

namespace delivery_website.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync(string userId);
        Task<AddressDto?> GetAddressByIdAsync(string userId, Guid addressId);
        Task<AddressDto?> GetDefaultAddressAsync(string userId);
        Task<AddressDto> CreateAddressAsync(string userId, CreateAddressDto dto);
        Task<AddressDto> UpdateAddressAsync(string userId, UpdateAddressDto dto);
        Task DeleteAddressAsync(string userId, Guid addressId);
        Task SetDefaultAddressAsync(string userId, Guid addressId);
    }
}