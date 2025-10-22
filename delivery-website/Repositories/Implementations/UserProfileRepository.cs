using delivery_website.Data;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace delivery_website.Repositories.Implementations
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public UserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByUserIdAsync(string userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task<UserProfile?> GetByIdAsync(Guid userProfileId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserProfileId == userProfileId);
        }

        public async Task<UserProfile> CreateAsync(UserProfile userProfile)
        {
            userProfile.CreatedDate = DateTime.UtcNow;
            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();
            return userProfile;
        }

        public async Task<UserProfile> UpdateAsync(UserProfile userProfile)
        {
            userProfile.UpdatedDate = DateTime.UtcNow;

            // More explicit update approach
            _context.Entry(userProfile).State = EntityState.Modified;

            // Explicitly mark DateOfBirth as modified if it has a value
            if (userProfile.DateOfBirth.HasValue)
            {
                _context.Entry(userProfile).Property(p => p.DateOfBirth).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return userProfile;
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            return await _context.UserProfiles.AnyAsync(up => up.UserId == userId);
        }
    }
}