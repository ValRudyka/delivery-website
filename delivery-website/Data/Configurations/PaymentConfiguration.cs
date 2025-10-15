using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.PaymentStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(p => p.Amount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(p => p.PaymentGateway)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.TransactionId)
                .HasMaxLength(255);

            builder.Property(p => p.RefundAmount)
                .HasPrecision(10, 2);

            builder.Property(p => p.RefundReason)
                .HasMaxLength(500);

            builder.Property(p => p.RefundTransactionId)
                .HasMaxLength(255);

            builder.HasIndex(p => p.OrderId).IsUnique();
            builder.HasIndex(p => p.TransactionId);
            builder.HasIndex(p => p.PaymentStatus);
            builder.HasIndex(p => p.PaymentDate);
        }
    }
}