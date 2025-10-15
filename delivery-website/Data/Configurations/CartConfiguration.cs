using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(c => c.CartId);

            builder.Property(c => c.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.ExpiresAt);
            builder.HasIndex(c => new { c.UserId, c.RestaurantId }).IsUnique();

            builder.Ignore(c => c.TotalAmount);

            builder.HasOne(c => c.Restaurant)
                .WithMany()
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}