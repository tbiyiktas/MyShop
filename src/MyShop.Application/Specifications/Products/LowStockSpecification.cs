using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;

namespace MyShop.Application.Specifications.Products;

public sealed class LowStockSpecification : Specification<Product>
{
    public int Threshold { get; }

    public LowStockSpecification(
        int threshold,
        bool orderByPriceDescending = false)
    {
        if (threshold < 0)
            throw new ArgumentOutOfRangeException(nameof(threshold));

        Threshold = threshold;

        Criteria = p => p.StockQuantity < threshold && p.IsActive;

        ApplyInclude(builder =>
        {
            builder.Include(p => p.Category);
        });

        if (orderByPriceDescending)
        {
            ApplyOrderByDescending(q => q.OrderByDescending(p => p.Price));
        }
        else
        {
            ApplyOrderBy(q => q
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name));
        }

        // Read-only query: disable change tracking for performance
        ApplyAsNoTracking();
    }
}
