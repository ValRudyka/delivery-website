using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.ToTable("MenuItems");

            builder.HasKey(m => m.MenuItemId);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Description)
                .IsRequired();

            builder.Property(m => m.Price)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(m => m.ImageUrl)
                .HasMaxLength(500);

            builder.Property(m => m.IsAvailable)
                .HasDefaultValue(true);

            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            builder.Property(m => m.Allergens)
                .HasColumnType("jsonb");

            builder.Property(m => m.NutritionalInfo)
                .HasColumnType("jsonb");

            builder.Property(m => m.DietaryTags)
                .HasColumnType("jsonb");

            builder.HasIndex(m => m.RestaurantId);
            builder.HasIndex(m => m.CategoryId);
            builder.HasIndex(m => m.IsAvailable);
            builder.HasIndex(m => m.Price);
            builder.HasIndex(m => new { m.RestaurantId, m.IsAvailable });

            builder.HasMany(m => m.OrderItems)
                .WithOne(oi => oi.MenuItem)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.CartItems)
                .WithOne(ci => ci.MenuItem)
                .HasForeignKey(ci => ci.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}