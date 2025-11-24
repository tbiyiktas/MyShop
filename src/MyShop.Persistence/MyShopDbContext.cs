using Microsoft.EntityFrameworkCore;
using MyShop.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MyShop.Persistence;

public class MyShopDbContext : DbContext
{
    public MyShopDbContext(DbContextOptions<MyShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(builder =>
        {
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18,2);

            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Category>(builder =>
        {
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(127);
        });
    }
}
