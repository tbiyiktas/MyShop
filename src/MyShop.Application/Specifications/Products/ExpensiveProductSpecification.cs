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

        AddInclude(p => p.Category);

        ApplyOrderByDescending(q => q.OrderByDescending(p => p.Price));
    }
}
