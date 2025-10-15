using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(ci => ci.CartItemId);

            builder.Property(ci => ci.Quantity)
                .IsRequired();

            builder.Property(ci => ci.UnitPrice)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(ci => ci.Customizations)
                .HasColumnType("jsonb");

            builder.HasIndex(ci => ci.CartId);
            builder.HasIndex(ci => ci.MenuItemId);

            builder.Ignore(ci => ci.TotalPrice);
        }
    }
}