using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.CategoryId);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.SortOrder)
                .HasDefaultValue(0);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(c => c.RestaurantId);
            builder.HasIndex(c => new { c.RestaurantId, c.SortOrder });

            builder.HasMany(c => c.MenuItems)
                .WithOne(m => m.Category)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}