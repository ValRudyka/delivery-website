using delivery_website.Models.DTOs.Address;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using delivery_website.Services.Interfaces;

namespace delivery_website.Services.Implementations
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<AddressService> _logger;

        public AddressService(
            IAddressRepository addressRepository,
            ILogger<AddressService> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(string userId)
        {
            var addresses = await _addressRepository.GetUserAddressesAsync(userId);
            return addresses.Select(MapToDto);
        }

        public async Task<AddressDto?> GetAddressByIdAsync(string userId, Guid addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.UserId != userId)
                return null;

            return MapToDto(address);
        }

        public async Task<AddressDto?> GetDefaultAddressAsync(string userId)
        {
            var address = await _addressRepository.GetDefaultAddressAsync(userId);
            return address == null ? null : MapToDto(address);
        }

        public async Task<AddressDto> CreateAddressAsync(string userId, CreateAddressDto dto)
        {
            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                UserId = userId,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                PostalCode = dto.PostalCode,
                IsDefault = dto.IsDefault,
                Label = dto.Label
            };

            var createdAddress = await _addressRepository.CreateAsync(address);
            return MapToDto(createdAddress);
        }

        public async Task<AddressDto> UpdateAddressAsync(string userId, UpdateAddressDto dto)
        {
            var address = await _addressRepository.GetByIdAsync(dto.AddressId);
            if (address == null || address.UserId != userId)
                throw new Exception("Address not found or access denied");

            address.AddressLine1 = dto.AddressLine1;
            address.AddressLine2 = dto.AddressLine2;
            address.City = dto.City;
            address.PostalCode = dto.PostalCode;
            address.IsDefault = dto.IsDefault;
            address.Label = dto.Label;

            var updatedAddress = await _addressRepository.UpdateAsync(address);
            return MapToDto(updatedAddress);
        }

        public async Task DeleteAddressAsync(string userId, Guid addressId)
        {
            if (!await _addressRepository.UserOwnsAddressAsync(userId, addressId))
                throw new Exception("Address not found or access denied");

            await _addressRepository.DeleteAsync(addressId);
        }

        public async Task SetDefaultAddressAsync(string userId, Guid addressId)
        {
            if (!await _addressRepository.UserOwnsAddressAsync(userId, addressId))
                throw new Exception("Address not found or access denied");

            await _addressRepository.SetDefaultAddressAsync(userId, addressId);
        }

        private AddressDto MapToDto(Address address)
        {
            return new AddressDto
            {
                AddressId = address.AddressId,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault,
                Label = address.Label
            };
        }
    }
}