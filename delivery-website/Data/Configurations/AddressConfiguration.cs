using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");

            builder.HasKey(a => a.AddressId);

            builder.Property(a => a.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(a => a.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.AddressLine2)
                .HasMaxLength(200);

            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.State)
                .HasMaxLength(100);

            builder.Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Ukraine");

            builder.Property(a => a.Latitude)
                .HasPrecision(10, 8);

            builder.Property(a => a.Longitude)
                .HasPrecision(11, 8);

            builder.Property(a => a.Label)
                .HasMaxLength(50);

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => new { a.UserId, a.IsDefault });

            builder.Ignore(a => a.FullAddress);
        }
    }
}