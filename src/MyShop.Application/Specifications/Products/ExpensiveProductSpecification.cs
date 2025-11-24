using System;
using System.Linq;
using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;

namespace MyShop.Application.Specifications.Products;

public sealed class ExpensiveProductSpecification : Specification<Product>
{
    public decimal MinPrice { get; }

    public ExpensiveProductSpecification(decimal minPrice)
    {
        if (minPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(minPrice));

        MinPrice = minPrice;

        Criteria = p => p.Price >= minPrice && p.IsActive;

        ApplyInclude(builder =>
        {
            builder.Include(p => p.Category);
        });

        ApplyOrderByDescending(q => q.OrderByDescending(p => p.Price));

        // Read-only query: disable change tracking for performance
        ApplyAsNoTracking();
    }
}
