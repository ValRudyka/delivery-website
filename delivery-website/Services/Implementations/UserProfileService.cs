using delivery_website.Models.DTOs.UserProfile;
using delivery_website.Repositories.Interfaces;
using delivery_website.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace delivery_website.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(
            IUserProfileRepository userProfileRepository,
            UserManager<IdentityUser> userManager,
            ILogger<UserProfileService> logger)
        {
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile == null)
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            return new UserProfileDto
            {
                UserProfileId = userProfile.UserProfileId,
                UserId = userProfile.UserId,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = user.Email!,
                PhoneNumber = userProfile.PhoneNumber,
                DateOfBirth = userProfile.DateOfBirth,
                ProfileImageUrl = userProfile.ProfileImageUrl,
                PreferredLanguage = userProfile.PreferredLanguage,
                CreatedDate = userProfile.CreatedDate
            };
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto dto)
        {
            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile == null)
                throw new Exception("User profile not found");

            // Update profile
            userProfile.FirstName = dto.FirstName;
            userProfile.LastName = dto.LastName;
            userProfile.PhoneNumber = dto.PhoneNumber;
            userProfile.DateOfBirth = dto.DateOfBirth;
            userProfile.PreferredLanguage = dto.PreferredLanguage;

            await _userProfileRepository.UpdateAsync(userProfile);

            return (await GetUserProfileAsync(userId))!;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }
    }
}