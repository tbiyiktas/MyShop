using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;

namespace MyShop.Application.Specifications.Products;

/// <summary>
/// Domain-specific builder for Product specifications with fluent API.
/// </summary>
public class ProductSpecificationBuilder : SpecificationBuilder<Product>
{
    /// <summary>
    /// Creates a new product specification builder.
    /// </summary>
    public new static ProductSpecificationBuilder Create() => new();

    /// <summary>
    /// Filters products by name (contains search).
    /// </summary>
    public ProductSpecificationBuilder WithName(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Where(p => p.Name.Contains(name));
        }
        return this;
    }

    /// <summary>
    /// Filters products by price range.
    /// </summary>
    public ProductSpecificationBuilder WithPriceRange(decimal? min, decimal? max)
    {
        if (min.HasValue)
            Where(p => p.Price >= min.Value);
        
        if (max.HasValue)
            Where(p => p.Price <= max.Value);
        
        return this;
    }

    /// <summary>
    /// Filters products by minimum price.
    /// </summary>
    public ProductSpecificationBuilder WithMinPrice(decimal minPrice)
    {
        Where(p => p.Price >= minPrice);
        return this;
    }

    /// <summary>
    /// Filters products by maximum price.
    /// </summary>
    public ProductSpecificationBuilder WithMaxPrice(decimal maxPrice)
    {
        Where(p => p.Price <= maxPrice);
        return this;
    }

    /// <summary>
    /// Filters products by category name.
    /// </summary>
    public ProductSpecificationBuilder WithCategory(string? categoryName)
    {
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            Where(p => p.Category.Name == categoryName);
            Include(p => p.Category);
        }
        return this;
    }

    /// <summary>
    /// Filters products by category ID.
    /// </summary>
    public ProductSpecificationBuilder WithCategoryId(int categoryId)
    {
        Where(p => p.CategoryId == categoryId);
        return this;
    }

    /// <summary>
    /// Filters only active products.
    /// </summary>
    public ProductSpecificationBuilder OnlyActive()
    {
        Where(p => p.IsActive);
        return this;
    }

    /// <summary>
    /// Filters only inactive products.
    /// </summary>
    public ProductSpecificationBuilder OnlyInactive()
    {
        Where(p => !p.IsActive);
        return this;
    }

    /// <summary>
    /// Filters products with low stock (below threshold).
    /// </summary>
    public ProductSpecificationBuilder WithLowStock(int threshold)
    {
        Where(p => p.StockQuantity < threshold);
        return this;
    }

    /// <summary>
    /// Filters products with stock above threshold.
    /// </summary>
    public ProductSpecificationBuilder WithMinStock(int minStock)
    {
        Where(p => p.StockQuantity >= minStock);
        return this;
    }

    /// <summary>
    /// Filters products that are in stock (quantity > 0).
    /// </summary>
    public ProductSpecificationBuilder InStock()
    {
        Where(p => p.StockQuantity > 0);
        return this;
    }

    /// <summary>
    /// Filters products that are out of stock.
    /// </summary>
    public ProductSpecificationBuilder OutOfStock()
    {
        Where(p => p.StockQuantity == 0);
        return this;
    }

    /// <summary>
    /// Includes Category navigation property.
    /// </summary>
    public ProductSpecificationBuilder WithCategoryIncluded()
    {
        Include(p => p.Category);
        return this;
    }

    /// <summary>
    /// Orders products by name ascending.
    /// </summary>
    public ProductSpecificationBuilder OrderByName()
    {
        OrderBy(p => p.Name);
        return this;
    }

    /// <summary>
    /// Orders products by name descending.
    /// </summary>
    public ProductSpecificationBuilder OrderByNameDescending()
    {
        OrderByDescending(p => p.Name);
        return this;
    }

    /// <summary>
    /// Orders products by price ascending.
    /// </summary>
    public ProductSpecificationBuilder OrderByPrice()
    {
        OrderBy(p => p.Price);
        return this;
    }

    /// <summary>
    /// Orders products by price descending.
    /// </summary>
    public ProductSpecificationBuilder OrderByPriceDescending()
    {
        OrderByDescending(p => p.Price);
        return this;
    }

    /// <summary>
    /// Orders products by stock quantity ascending.
    /// </summary>
    public ProductSpecificationBuilder OrderByStock()
    {
        OrderBy(p => p.StockQuantity);
        return this;
    }

    /// <summary>
    /// Orders products by stock quantity descending.
    /// </summary>
    public ProductSpecificationBuilder OrderByStockDescending()
    {
        OrderByDescending(p => p.StockQuantity);
        return this;
    }

    /// <summary>
    /// Enables AsNoTracking for read-only queries (performance optimization).
    /// </summary>
    public new ProductSpecificationBuilder AsNoTracking()
    {
        base.AsNoTracking();
        return this;
    }

    /// <summary>
    /// Bypasses global query filters (e.g., soft delete, tenant filters).
    /// </summary>
    public new ProductSpecificationBuilder IgnoreQueryFilters()
    {
        base.IgnoreQueryFilters();
        return this;
    }
}
