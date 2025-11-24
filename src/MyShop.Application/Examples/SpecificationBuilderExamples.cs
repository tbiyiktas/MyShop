using MyShop.Application.Specifications.Products;
using MyShop.Domain.Entities;

namespace MyShop.Application.Examples;

/// <summary>
/// Examples demonstrating how to use the Specification Builder pattern.
/// </summary>
public static class SpecificationBuilderExamples
{
    /// <summary>
    /// Example 1: Simple product search with name filter
    /// </summary>
    public static void Example1_SimpleSearch()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithName("Laptop")
            .OnlyActive()
            .WithCategoryIncluded()
            .OrderByName()
            .AsNoTracking()
            .Build();

        // Use with repository:
        // var products = await _repository.ListAsync(spec, 1, 20);
    }

    /// <summary>
    /// Example 2: Price range filter
    /// </summary>
    public static void Example2_PriceRange()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithPriceRange(min: 500, max: 2000)
            .OnlyActive()
            .InStock()
            .OrderByPriceDescending()
            .AsNoTracking()
            .Build();
    }

    /// <summary>
    /// Example 3: Low stock products
    /// </summary>
    public static void Example3_LowStock()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithLowStock(threshold: 10)
            .OnlyActive()
            .WithCategoryIncluded()
            .OrderByStock()
            .AsNoTracking()
            .Build();
    }

    /// <summary>
    /// Example 4: Category-specific products
    /// </summary>
    public static void Example4_CategoryFilter()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithCategory("Electronics")
            .OnlyActive()
            .WithMinPrice(100)
            .OrderByPriceDescending()
            .AsNoTracking()
            .Build();
    }

    /// <summary>
    /// Example 5: Dynamic filters based on user input
    /// </summary>
    public static void Example5_DynamicFilters(
        string? searchTerm,
        decimal? minPrice,
        decimal? maxPrice,
        string? category,
        bool? onlyInStock)
    {
        var builder = ProductSpecificationBuilder.Create();

        // Add filters conditionally
        if (!string.IsNullOrWhiteSpace(searchTerm))
            builder.WithName(searchTerm);

        if (minPrice.HasValue || maxPrice.HasValue)
            builder.WithPriceRange(minPrice, maxPrice);

        if (!string.IsNullOrWhiteSpace(category))
            builder.WithCategory(category);

        if (onlyInStock == true)
            builder.InStock();

        var spec = builder
            .OnlyActive()
            .OrderByName()
            .AsNoTracking()
            .Build();
    }

    /// <summary>
    /// Example 6: Complex query with multiple conditions
    /// </summary>
    public static void Example6_ComplexQuery()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithName("Gaming")
            .WithPriceRange(min: 1000, max: 3000)
            .WithCategory("Electronics")
            .InStock()
            .OnlyActive()
            .OrderByPriceDescending()
            .AsNoTracking()
            .Build();
    }

    /// <summary>
    /// Example 7: Admin query with deleted items (IgnoreQueryFilters)
    /// </summary>
    public static void Example7_AdminQuery()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithCategory("Electronics")
            .IgnoreQueryFilters() // Include soft-deleted items
            .WithCategoryIncluded()
            .OrderByName()
            .Build(); // No AsNoTracking - we might need to restore items
    }

    /// <summary>
    /// Example 8: Out of stock products for reordering
    /// </summary>
    public static void Example8_OutOfStock()
    {
        var spec = ProductSpecificationBuilder.Create()
            .OutOfStock()
            .OnlyActive()
            .WithCategoryIncluded()
            .OrderByName()
            .AsNoTracking()
            .Build();
    }
}
