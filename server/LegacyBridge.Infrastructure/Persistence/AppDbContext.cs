using Microsoft.EntityFrameworkCore;
using LegacyBridge.Domain.Entities;

namespace LegacyBridge.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSets para as entidades
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da tabela customers
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Document).HasColumnName("document").HasMaxLength(20);
            entity.Property(e => e.Active).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // Configuração da tabela products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(12,2)");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}