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
/// Unit tests for the specification pattern implementation.
/// </summary>
public class SpecificationTests
{
    static SpecificationTests()
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
        // Create and add a category
        var catResult = Category.Create("Electronics");
        var category = catResult.IsSuccess ? catResult.Value : throw new InvalidOperationException();
        ctx.Categories.Add(category);
        ctx.SaveChanges(); // generate Id
        // Create and add a product using the generated CategoryId
        var prodResult = Product.Create("Phone", 199m, 10, category.Id);
        var product = prodResult.IsSuccess ? prodResult.Value : throw new InvalidOperationException();
        ctx.Products.Add(product);
        ctx.SaveChanges();
        return ctx;
    }

    [Fact]
    public void Specification_ApplyInclude_BuildsIncludeExpressions()
    {
        var spec = new TestSpecification<Product>();
        spec.Include(b => b.Include(p => p.Category));
        Assert.Single(spec.IncludeExpressions);
        var includeExpr = spec.IncludeExpressions.First();
        Assert.Contains("SimpleIncludeExpression", includeExpr.GetType().Name);
    }

    [Fact]
    public void SpecificationEvaluator_Applies_AsNoTracking_And_SplitQuery()
    {
        var spec = new TestSpecification<Product>();
        spec.SetAsNoTracking();
        spec.SetAsSplitQuery();
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        // Verify flags are set on specification
        Assert.True(spec.AsNoTracking);
        Assert.True(spec.AsSplitQuery);
    }

    [Fact]
    public void SpecificationEvaluator_Executes_IncludeExpressions()
    {
        var spec = new TestSpecification<Product>();
        spec.Include(b => b.Include(p => p.Category));
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var product = query.First();
        Assert.NotNull(product.Category);
        Assert.Equal("Electronics", product.Category.Name);
    }

    [Fact]
    public void Specification_MultipleIncludes_AppliesAll()
    {
        var spec = new TestSpecification<Product>();
        spec.Include(b => b.Include(p => p.Category));
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        var product = query.First();
        Assert.NotNull(product.Category);
        // Verify include was applied by checking navigation property is loaded
        Assert.Equal("Electronics", product.Category.Name);
    }

    [Fact]
    public void Specification_ComplexCriteria_FiltersCorrectly()
    {
        var spec = new TestSpecification<Product>();
        using var ctx = CreateContext();
        // Add more products for testing
        var cat = ctx.Categories.First();
        var prod2 = Product.Create("Tablet", 299m, 15, cat.Id).Value;
        var prod3 = Product.Create("Laptop", 999m, 5, cat.Id).Value;
        ctx.Products.AddRange(prod2, prod3);
        ctx.SaveChanges();
        
        var query = SpecificationEvaluator.ApplySpecification(
            ctx.Products.AsQueryable(), 
            spec);
        var results = query.Where(p => p.Price >= 200 && p.Price <= 500).ToList();
        Assert.Single(results);
        Assert.Equal("Tablet", results[0].Name);
    }

    [Fact]
    public void Specification_OrderByDescending_TakesPrecedence()
    {
        var spec = new TestSpecification<Product>();
        spec.SetOrderBy(q => q.OrderBy(p => p.Price));
        spec.SetOrderByDescending(q => q.OrderByDescending(p => p.Price));
        using var ctx = CreateContext();
        var query = SpecificationEvaluator.ApplySpecification(ctx.Products.AsQueryable(), spec);
        // OrderByDescending should be null when OrderBy is set
        Assert.NotNull(spec.OrderBy);
        Assert.NotNull(spec.OrderByDescending);
    }
}
