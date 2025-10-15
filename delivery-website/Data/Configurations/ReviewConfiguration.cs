using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.ReviewId);

            builder.Property(r => r.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.ReviewText)
                .IsRequired();

            builder.Property(r => r.ModeratorId)
                .HasMaxLength(450);

            builder.Property(r => r.ModerationNotes)
                .HasMaxLength(500);

            builder.Property(r => r.ImageUrls)
                .HasColumnType("jsonb");

            builder.HasIndex(r => new { r.UserId, r.RestaurantId }).IsUnique();
            builder.HasIndex(r => r.RestaurantId);
            builder.HasIndex(r => r.UserId);
            builder.HasIndex(r => r.Rating);
            builder.HasIndex(r => r.IsApproved);
            builder.HasIndex(r => r.CreatedDate);
        }
    }
}