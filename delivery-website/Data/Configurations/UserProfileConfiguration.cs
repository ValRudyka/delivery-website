using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(u => u.UserProfileId);

            builder.Property(u => u.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.PreferredLanguage)
                .HasMaxLength(10)
                .HasDefaultValue("uk");

            builder.Property(u => u.ProfileImageUrl)
                .HasMaxLength(500);

            builder.HasIndex(u => u.UserId).IsUnique();
            builder.HasIndex(u => u.PhoneNumber);

            builder.Ignore(u => u.FullName);
        }
    }
}