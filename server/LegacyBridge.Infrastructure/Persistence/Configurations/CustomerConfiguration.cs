using LegacyBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyBridge.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(c => c.Document)
            .HasMaxLength(22)
            .HasColumnName("document");

        builder.Property(c => c.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.Document)
            .IsUnique()
            .HasDatabaseName("idx_customers_document");
    }
}