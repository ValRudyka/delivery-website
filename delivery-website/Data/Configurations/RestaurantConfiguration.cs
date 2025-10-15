using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.ToTable("Restaurants");

            builder.HasKey(r => r.RestaurantId);

            builder.Property(r => r.OwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Description)
                .IsRequired();

            builder.Property(r => r.CuisineType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.MinimumOrderAmount)
                .HasPrecision(10, 2);

            builder.Property(r => r.DeliveryFee)
                .HasPrecision(10, 2);

            builder.Property(r => r.DeliveryRadius)
                .HasPrecision(10, 2);

            builder.Property(r => r.AverageRating)
                .HasPrecision(3, 2)
                .HasDefaultValue(0);

            builder.Property(r => r.TotalReviews)
                .HasDefaultValue(0);

            builder.Property(r => r.OpeningHours)
                .HasColumnType("jsonb");

            builder.HasIndex(r => r.OwnerId);
            builder.HasIndex(r => r.CuisineType);
            builder.HasIndex(r => r.IsActive);
            builder.HasIndex(r => r.IsApproved);
            builder.HasIndex(r => r.AverageRating);
            builder.HasIndex(r => r.City);

            builder.Ignore(r => r.FullAddress);

            builder.HasMany(r => r.Categories)
                .WithOne(c => c.Restaurant)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.MenuItems)
                .WithOne(m => m.Restaurant)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Orders)
                .WithOne(o => o.Restaurant)
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Reviews)
                .WithOne(rv => rv.Restaurant)
                .HasForeignKey(rv => rv.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}