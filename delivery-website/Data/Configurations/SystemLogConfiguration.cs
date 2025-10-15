using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using delivery_website.Models.Entities;

namespace delivery_website.Data.Configurations
{
    public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
    {
        public void Configure(EntityTypeBuilder<SystemLog> builder)
        {
            builder.ToTable("SystemLogs");

            builder.HasKey(l => l.LogId);

            builder.Property(l => l.LogLevel)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.Message)
                .IsRequired();

            builder.Property(l => l.UserId)
                .HasMaxLength(450);

            builder.Property(l => l.RequestPath)
                .HasMaxLength(500);

            builder.HasIndex(l => l.LogLevel);
            builder.HasIndex(l => l.CreatedDate);
        }
    }
}