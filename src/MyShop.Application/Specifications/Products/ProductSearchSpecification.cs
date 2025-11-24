using System.Collections.Generic;
using MyShop.Application.Common;
using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;

namespace MyShop.Application.Specifications.Products;

public sealed class ProductSearchSpecification : Specification<Product>
{
    public ProductSearchSpecification(
        string? search,
        bool? isActive,
        decimal? minPrice,
        decimal? maxPrice,
        string? categoryName,
        IReadOnlyList<MyShop.Contracts.Common.SortCriterion>? sorts)
    {
        var filters = new List<FilterCriterion>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(new FilterCriterion(
                propertyPath: nameof(Product.Name),
                operation: MyShop.Contracts.Common.FilterOperation.Contains,
                value: search,
                caseInsensitive: true));
        }

        if (isActive.HasValue)
        {
            filters.Add(new FilterCriterion(
                propertyPath: nameof(Product.IsActive),
                operation: MyShop.Contracts.Common.FilterOperation.Equals,
                value: isActive.Value));
        }

        if (minPrice.HasValue)
        {
            filters.Add(new FilterCriterion(
                propertyPath: nameof(Product.Price),
                operation: MyShop.Contracts.Common.FilterOperation.GreaterThanOrEqual,
                value: minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            filters.Add(new FilterCriterion(
                propertyPath: nameof(Product.Price),
                operation: MyShop.Contracts.Common.FilterOperation.LessThanOrEqual,
                value: maxPrice.Value));
        }

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            // Nested property path: "Category.Name"
            filters.Add(new FilterCriterion(
                propertyPath: "Category.Name",
                operation: MyShop.Contracts.Common.FilterOperation.Equals,
                value: categoryName,
                caseInsensitive: true));
        }

        // Tek bir predicate'e çevir
        Criteria = ExpressionBuilder.BuildAndPredicate<Product>(filters);

        // Category'yi her durumda include et (mapping için)
        ApplyInclude(builder =>
        {
            builder.Include(p => p.Category);
        });

        // SORT
        if (sorts is not null && sorts.Count > 0)
        {
            // Multi-sort: örn. Price desc, Name asc, Category.Name asc
            ApplyOrderBy(ExpressionBuilder.BuildOrderBy<Product>(sorts));
        }
        else
        {
            // Default: Name asc
            ApplyOrderBy(q => q.OrderBy(p => p.Name));
        }

        // Read-only query: disable change tracking for performance
        ApplyAsNoTracking();
    }

}
