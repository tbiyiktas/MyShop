using MyShop.Application.Specifications.Base;
using MyShop.Application.Specifications.Products;
using MyShop.Domain.Entities;

namespace MyShop.Application.Examples;

/// <summary>
/// Examples demonstrating ThenBy multi-sort and composition with AsNoTracking/IgnoreQueryFilters.
/// </summary>
public static class SpecificationImprovementsExamples
{
    /// <summary>
    /// Example 1: Multi-sort with ThenBy
    /// </summary>
    public static void Example1_MultiSort()
    {
        var spec = ProductSpecificationBuilder.Create()
            .OnlyActive()
            .OrderBy(p => p.Category.Name)  // Primary sort: Category
            .ThenBy(p => p.Price)            // Secondary sort: Price
            .ThenByDescending(p => p.Name)   // Tertiary sort: Name DESC
            .AsNoTracking()
            .Build();

        // SQL: ORDER BY Category.Name ASC, Price ASC, Name DESC
    }

    /// <summary>
    /// Example 2: Composition with AsNoTracking preserved
    /// </summary>
    public static void Example2_CompositionWithAsNoTracking()
    {
        var activeSpec = ProductSpecificationBuilder.Create()
            .OnlyActive()
            .AsNoTracking()  // AsNoTracking enabled
            .Build();

        var inStockSpec = ProductSpecificationBuilder.Create()
            .InStock()
            .Build();  // No AsNoTracking

        // Composition: AsNoTracking is preserved (OR logic)
        var combinedSpec = activeSpec.And(inStockSpec);
        
        // Result: AsNoTracking = true (because left has it)
    }

    /// <summary>
    /// Example 3: Complex multi-sort scenario
    /// </summary>
    public static void Example3_ComplexMultiSort()
    {
        var spec = SpecificationBuilder<Product>.Create()
            .Where(p => p.IsActive)
            .Where(p => p.StockQuantity > 0)
            .Include(p => p.Category)
            .OrderBy(p => p.Category.Name)      // 1st: Category
            .ThenByDescending(p => p.Price)     // 2nd: Price DESC
            .ThenBy(p => p.StockQuantity)       // 3rd: Stock
            .ThenBy(p => p.Name)                // 4th: Name
            .AsNoTracking()
            .Build();

        // SQL: ORDER BY Category.Name ASC, Price DESC, StockQuantity ASC, Name ASC
    }

    /// <summary>
    /// Example 4: IgnoreQueryFilters composition
    /// </summary>
    public static void Example4_IgnoreQueryFiltersComposition()
    {
        var electronicsSpec = new ProductByCategoryNameSpecification("Electronics");
        
        var adminSpec = ProductSpecificationBuilder.Create()
            .IgnoreQueryFilters()  // Include soft-deleted items
            .Build();

        // Composition: IgnoreQueryFilters is preserved
        var combinedSpec = electronicsSpec.And(adminSpec);
        
        // Result: IgnoreQueryFilters = true
    }

    /// <summary>
    /// Example 5: Price range with multi-sort
    /// </summary>
    public static void Example5_PriceRangeMultiSort()
    {
        var spec = ProductSpecificationBuilder.Create()
            .WithPriceRange(min: 100, max: 1000)
            .OnlyActive()
            .InStock()
            .WithCategoryIncluded()
            .OrderBy(p => p.Category.Name)
            .ThenByDescending(p => p.Price)
            .ThenBy(p => p.Name)
            .AsNoTracking()
            .Build();

        // Products grouped by category, then by price (high to low), then by name
    }
}
