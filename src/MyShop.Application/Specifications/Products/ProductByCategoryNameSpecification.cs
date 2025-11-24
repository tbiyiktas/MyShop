using System;
using System.Linq;
using MyShop.Application.Specifications.Base;
using MyShop.Domain.Entities;

namespace MyShop.Application.Specifications.Products;

public sealed class ProductByCategoryNameSpecification : Specification<Product>
{
    public string CategoryName { get; }

    public ProductByCategoryNameSpecification(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("Category name is required.", nameof(categoryName));

        CategoryName = categoryName;

        // WHERE: kategori adı + aktif ürün
        Criteria = p => p.Category.Name == categoryName && p.IsActive;

        // INCLUDE: Category
        ApplyInclude(builder =>
        {
            builder.Include(p => p.Category);
        });

        // ORDER: isim
        ApplyOrderBy(q => q.OrderBy(p => p.Name));

        // Read-only query: disable change tracking for performance
        ApplyAsNoTracking();
    }
}
