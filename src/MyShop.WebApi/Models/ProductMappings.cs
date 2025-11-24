using MyShop.Contracts.Common;
using MyShop.Contracts.Products;
using MyShop.Domain.Entities;

namespace MyShop.WebApi.Models;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
        => new()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name
        };

    public static PaginatedResponse<ProductDto> ToDtoPage(
        this PaginatedResponse<Product> page)
        => PaginatedResponse<ProductDto>.Create(
            page.Items.Select(p => p.ToDto()).ToList(),
            page.TotalCount,
            page.PageIndex,
            page.PageSize);
}
