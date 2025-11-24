using MyShop.Application.Abstractions;
using MyShop.Contracts.Common;
using MyShop.Domain.Common;
using MyShop.Domain.Entities;

namespace MyShop.Application.Services;

public class CategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<PaginatedResponse<Category>> GetAllCategoriesAsync(
        int pageIndex,
        int pageSize)
        => _categoryRepository.ListAllAsync(pageIndex, pageSize);

    public Task<Category?> GetByIdAsync(int id)
        => _categoryRepository.GetByIdAsync(id);

    public async Task<Result<Category>> CreateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = Category.Create(name);

        if (result.HasError)
        {
            return result;
        }

        var category = result.Value!;

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Category>.Success(category);
    }

    public async Task<Result<Category>> UpdateAsync(
        int id,
        string name,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return Result<Category>.Failed("Category not found.");
        }

        var changeResult = category.ChangeName(name);
        if (changeResult.HasError)
        {
            return Result<Category>.Failed(changeResult.Errors);
        }

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Category>.Success(category);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return Result.Failed("Category not found.");
        }

        await _categoryRepository.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
