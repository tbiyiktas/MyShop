using MyShop.Application.Common;
using MyShop.Application.Specifications.Base;
using MyShop.Contracts.Common;
using MyShop.Domain.Entities;

namespace MyShop.Application.Specifications.Products;

/// <summary>
/// Generic grid filtresiyle Product sorgulayan specification.
/// </summary>
public sealed class ProductGridSearchSpecification : Specification<Product>
{
    public ProductGridSearchSpecification(GridFilterRequestDto request)
    {
        // 1) Predicate'i grid filter builder ile oluştur
        Criteria = GridFilterExpressionBuilder.BuildPredicate<Product>(request);

        // 2) Category'yi her zaman include edelim (Category.Name kullanıldığı için)
        ApplyInclude(builder =>
        {
            builder.Include(p => p.Category);
        });

        // 3) Sort'ları mapping edip multi-sort uygula
        var sorts = request.Sorts?.ToSortCriteria();

        if (sorts is not null && sorts.Count > 0)
        {
            ApplyOrderBy(ExpressionBuilder.BuildOrderBy<Product>(sorts));
        }
        else
        {
            ApplyOrderBy(q => q.OrderBy(p => p.Name));
        }

        // Read-only query: disable change tracking for performance
        ApplyAsNoTracking();
    }
}
