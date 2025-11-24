using MyShop.Application.Abstractions;
using MyShop.Application.Common;
using MyShop.Application.Specifications.Base;
using MyShop.Application.Specifications.Products;
using MyShop.Contracts.Common;
using MyShop.Domain.Entities;
using System.Globalization;

namespace MyShop.Application.Services;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<PaginatedResponse<Product>> GetLowStockProductsAsync(
        int threshold,
        int pageIndex,
        int pageSize,
        bool orderByPriceDescending = false)
    {
        var spec = new LowStockSpecification(threshold, orderByPriceDescending);
        return _productRepository.ListAsync(spec, pageIndex, pageSize);
    }

    public Task<PaginatedResponse<Product>> GetAllProductsAsync(
        int pageIndex,
        int pageSize)
        => _productRepository.ListAllAsync(pageIndex, pageSize);

    public Task<Product?> GetByIdAsync(int id)
        => _productRepository.GetByIdAsync(id);

    public async Task<Product> CreateAsync(
        string name,
        decimal price,
        int stockQuantity,
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = Product.Create(name, price, stockQuantity, categoryId, isActive: true);

        if (result.HasError)
        {

        }
        var product = result.Value;

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<Product?> UpdateAsync(
        int id,
        string name,
        decimal price,
        int stockQuantity,
        int categoryId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return null;

        var updateResult = product.Update(name, price, stockQuantity, categoryId);
        if (updateResult.HasError)
        {
            throw new InvalidOperationException(string.Join(", ", updateResult.Errors));
        }

        if (isActive)
            product.Activate();
        else
            product.Deactivate();

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return false;

        await _productRepository.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }


    public async Task<PaginatedResponse<Product>> GetLowStockExpensiveNonElectronicsAsync(
    int stockThreshold,
    decimal minPrice,
    int pageIndex,
    int pageSize)
    {
        var lowStock = new LowStockSpecification(stockThreshold);
        var expensive = new ExpensiveProductSpecification(minPrice);
        var inElectronics = new ProductByCategoryNameSpecification("Electronics");

        var spec = lowStock
            .And(expensive)
            .And(inElectronics.Not());

        return await _productRepository.ListAsync(spec, pageIndex, pageSize);
    }

    public Task<PaginatedResponse<Product>> SearchAsync(
        string? search,
        bool? isActive,
        decimal? minPrice,
        decimal? maxPrice,
        string? categoryName,
        IReadOnlyList<SortCriterion>? sorts,
        int pageIndex,
        int pageSize)
    {
        var spec = new ProductSearchSpecification(
            search: search,
            isActive: isActive,
            minPrice: minPrice,
            maxPrice: maxPrice,
            categoryName: categoryName,
            sorts: sorts);

        return _productRepository.ListAsync(spec, pageIndex, pageSize);
    }

    public Task<PaginatedResponse<Product>> SearchWithGridAsync(
        GridFilterRequestDto request)
    {
        var spec = new ProductGridSearchSpecification(request);

        return _productRepository.ListAsync(spec, request.PageIndex, request.PageSize);
    }
}
