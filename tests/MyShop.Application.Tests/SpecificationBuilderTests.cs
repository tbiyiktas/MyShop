using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;
using MyShop.Persistence.Specifications;
using Xunit;

namespace MyShop.Application.Tests;

/// <summary>
/// Unit tests for the SpecificationBuilder fluent API.
/// </summary>
public class SpecificationBuilderTests
{
    static SpecificationBuilderTests()
    {
        // Initialize IncludeBuilder with factory for tests
        var factory = new IncludeExpressionFactory();
        IncludeBuilder.Initialize(factory);
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
    }

    private TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new TestDbContext(options);
        
        // Create category
        var catResult = Category.Create("Electronics");
        var category = catResult.IsSuccess ? catResult.Value : throw new InvalidOperationException();
        ctx.Categories.Add(category);
        ctx.SaveChanges();
        
        // Create products
        var prod1 = Product.Create("Phone", 199m, 5, category.Id).Value;
        var prod2 = Product.Create("Laptop", 999m, 2, category.Id).Value;
        var prod3 = Product.Create("Tablet", 299m, 15, category.Id).Value;
        ctx.Products.AddRange(prod1, prod2, prod3);
        ctx.SaveChanges();
        
        return ctx;
    }

    [Fact]
    public void Builder_MultipleWhereClauses_CombinesWithAnd()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .Where(p => p.Price >= 200)
            .Where(p => p.StockQuantity < 10)
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();

        // Should match Laptop (price=999, stock=2)
        Assert.Single(results);
        Assert.Equal("Laptop", results[0].Name);
    }

    [Fact]
    public void Builder_WithInclude_LoadsNavigationProperty()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .Include(p => p.Category)
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var product = query.First();

        Assert.NotNull(product.Category);
        Assert.Equal("Electronics", product.Category.Name);
    }

    [Fact]
    public void Builder_OrderByAndThenBy_SortsCorrectly()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .OrderBy(p => p.Price)
            .ThenBy(p => p.Name)
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();

        // Should be ordered by price ascending
        Assert.Equal("Phone", results[0].Name);   // 199
        Assert.Equal("Tablet", results[1].Name);  // 299
        Assert.Equal("Laptop", results[2].Name);  // 999
    }

    [Fact]
    public void Builder_OrderByDescending_SortsDescending()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .OrderByDescending(p => p.Price)
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();

        // Should be ordered by price descending
        Assert.Equal("Laptop", results[0].Name);  // 999
        Assert.Equal("Tablet", results[1].Name);  // 299
        Assert.Equal("Phone", results[2].Name);   // 199
    }

    [Fact]
    public void Builder_AsNoTracking_SetsFlag()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .AsNoTracking()
            .Build();

        Assert.True(spec.AsNoTracking);
    }

    [Fact]
    public void Builder_IgnoreQueryFilters_SetsFlag()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .IgnoreQueryFilters()
            .Build();

        Assert.True(spec.IgnoreQueryFilters);
    }

    [Fact]
    public void Builder_CombinedFeatures_WorksTogether()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .Where(p => p.Price >= 200)
            .Include(p => p.Category)
            .OrderBy(p => p.Price)
            .AsNoTracking()
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();

        // Should match Tablet and Laptop, ordered by price
        Assert.Equal(2, results.Count);
        Assert.Equal("Tablet", results[0].Name);
        Assert.Equal("Laptop", results[1].Name);
        Assert.NotNull(results[0].Category);
        Assert.True(spec.AsNoTracking);
    }

    [Fact]
    public void Builder_EmptyBuilder_ProducesValidSpec()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();

        // Should return all products
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Builder_ThenByDescending_ChainsCorrectly()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .OrderBy(p => p.Price)
            .ThenByDescending(p => p.Name)
            .Build();

        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();

        // Should be ordered by price, then by name descending
        Assert.Equal(3, results.Count);
        Assert.Equal("Phone", results[0].Name);
    }
}
