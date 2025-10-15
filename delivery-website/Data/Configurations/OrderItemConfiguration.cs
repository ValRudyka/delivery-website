using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(oi => oi.OrderItemId);

            builder.Property(oi => oi.MenuItemName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            builder.Property(oi => oi.UnitPrice)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(oi => oi.TotalPrice)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(oi => oi.Customizations)
                .HasColumnType("jsonb");

            builder.HasIndex(oi => oi.OrderId);
            builder.HasIndex(oi => oi.MenuItemId);
        }
    }
}