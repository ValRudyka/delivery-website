using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.OrderId);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(o => o.OrderStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(o => o.SubtotalAmount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(o => o.TaxAmount)
                .HasPrecision(10, 2);

            builder.Property(o => o.DeliveryFee)
                .HasPrecision(10, 2);

            builder.Property(o => o.DiscountAmount)
                .HasPrecision(10, 2);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(o => o.DeliveryInstructions)
                .HasMaxLength(500);

            builder.Property(o => o.CancellationReason)
                .HasMaxLength(500);

            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.RestaurantId);
            builder.HasIndex(o => o.OrderStatus);
            builder.HasIndex(o => o.OrderDate);
            builder.HasIndex(o => new { o.UserId, o.OrderDate });

            builder.HasOne(o => o.DeliveryAddress)
                .WithMany()
                .HasForeignKey(o => o.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Review)
                .WithOne(r => r.Order)
                .HasForeignKey<Review>(r => r.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
