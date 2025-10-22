using delivery_website.Models.Entities;

namespace delivery_website.Repositories.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(string userId);
        Task<UserProfile?> GetByIdAsync(Guid userProfileId);
        Task<UserProfile> CreateAsync(UserProfile userProfile);
        Task<UserProfile> UpdateAsync(UserProfile userProfile);
        Task<bool> ExistsAsync(string userId);
    }
}