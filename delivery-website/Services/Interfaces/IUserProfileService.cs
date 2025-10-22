using delivery_website.Models.DTOs.UserProfile;

namespace delivery_website.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto dto);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}