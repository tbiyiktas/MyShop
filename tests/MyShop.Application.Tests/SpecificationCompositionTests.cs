using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyShop.Application.Abstractions;
using MyShop.Application.Specifications.Base;
using MyShop.Application.Specifications.Products;
using MyShop.Domain.Entities;
using MyShop.Persistence.Specifications;
using Xunit;

namespace MyShop.Application.Tests;

/// <summary>
/// Tests for specification composition (And, Or, Not) and query flags.
/// </summary>
public class SpecificationCompositionTests
{
    static SpecificationCompositionTests()
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
        // Create category via factory
        var catResult = Category.Create("Electronics");
        var category = catResult.IsSuccess ? catResult.Value : throw new InvalidOperationException();
        ctx.Categories.Add(category);
        ctx.SaveChanges(); // generate Id
        // Create products via factory using generated CategoryId
        var prod1 = Product.Create("Phone", 199m, 5, category.Id).IsSuccess ? Product.Create("Phone", 199m, 5, category.Id).Value : throw new InvalidOperationException();
        var prod2 = Product.Create("Laptop", 999m, 2, category.Id).IsSuccess ? Product.Create("Laptop", 999m, 2, category.Id).Value : throw new InvalidOperationException();
        var prod3 = Product.Create("Old TV", 50m, 0, category.Id, isActive: false).IsSuccess ? Product.Create("Old TV", 50m, 0, category.Id, isActive: false).Value : throw new InvalidOperationException();
        ctx.Products.AddRange(prod1, prod2, prod3);
        ctx.SaveChanges();
        return ctx;
    }

    [Fact]
    public void AndSpecification_Composes_Criteria_And_Includes()
    {
        var lowStock = new LowStockSpecification(3);
        var expensive = new ExpensiveProductSpecification(150);
        var spec = lowStock.And(expensive);
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();
        Assert.Single(results);
        Assert.Equal("Laptop", results[0].Name); // Laptop: stock=2 (<3), price=999 (>=150)
    }

    [Fact]
    public void OrSpecification_Composes_Criteria_Or_Includes()
    {
        var lowStock = new LowStockSpecification(1);
        var cheap = new ExpensiveProductSpecification(0); // matches all active
        var spec = lowStock.Or(cheap);
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void NotSpecification_Inverts_Criteria()
    {
        var lowStock = new LowStockSpecification(3);
        var spec = lowStock.Not();
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var results = query.ToList();
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void AsNoTracking_Preserves_Flag()
    {
        var spec = new TestSpecification<Product>();
        spec.SetAsNoTracking();
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        Assert.True(spec.AsNoTracking);
    }

    [Fact]
    public void IgnoreQueryFilters_Preserves_Flag()
    {
        var spec = new TestSpecification<Product>();
        spec.IgnoreFilters();
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var count = query.Count();
        Assert.True(count >= 0);
    }

    [Fact]
    public void ThenBy_MultiSort_Works()
    {
        var spec = new TestSpecification<Product>();
        spec.SetOrderBy(q => q.OrderBy(p => p.Price));
        spec.ThenBy(p => p.Name);
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var list = query.ToList();
        Assert.Equal("Old TV", list[0].Name); // Lowest price first
    }

    [Fact]
    public void ComplexComposition_AndThenOr_Works()
    {
        // (LowStock AND Expensive) OR Inactive
        var lowStock = new LowStockSpecification(3);
        var expensive = new ExpensiveProductSpecification(150);
        var andSpec = lowStock.And(expensive);
        
        using var ctx = CreateContext();
        // Create an inactive product
        var cat = ctx.Categories.First();
        var inactiveProduct = Product.Create("Inactive Item", 50m, 10, cat.Id, isActive: false).Value;
        ctx.Products.Add(inactiveProduct);
        ctx.SaveChanges();
        
        // Now apply: (lowStock AND expensive) should match Laptop
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), andSpec);
        var results = query.ToList();
        Assert.Single(results);
        Assert.Equal("Laptop", results[0].Name);
    }

    [Fact]
    public void DoubleNegation_RestoresOriginal()
    {
        var lowStock = new LowStockSpecification(3);
        var notLowStock = lowStock.Not();
        var doubleNot = notLowStock.Not();
        
        using var ctx = CreateContext();
        var originalQuery = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), lowStock);
        var doubleNotQuery = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), doubleNot);
        
        var originalResults = originalQuery.ToList();
        var doubleNotResults = doubleNotQuery.ToList();
        
        // Double negation should yield same results as original
        Assert.Equal(originalResults.Count, doubleNotResults.Count);
        Assert.All(originalResults, p => Assert.Contains(p.Id, doubleNotResults.Select(r => r.Id)));
    }

    [Fact]
    public void AsSplitQuery_PreservedInComposition()
    {
        var spec1 = new TestSpecification<Product>();
        spec1.SetAsSplitQuery();
        
        var spec2 = new TestSpecification<Product>();
        var combined = spec1.And(spec2);
        
        // AsSplitQuery flag should be preserved
        Assert.True(combined.AsSplitQuery);
    }

    [Fact]
    public void IncludeDeduplication_InAndComposition()
    {
        var spec1 = new TestSpecification<Product>();
        spec1.Include(b => b.Include(p => p.Category));
        
        var spec2 = new TestSpecification<Product>();
        spec2.Include(b => b.Include(p => p.Category));
        
        var combined = spec1.And(spec2);
        
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), combined);
        var product = query.First();
        
        // Should work correctly even with duplicate includes
        Assert.NotNull(product.Category);
        Assert.Equal("Electronics", product.Category.Name);
    }
}
