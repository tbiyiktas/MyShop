using MyShop.Application.Common;
using MyShop.Contracts.Common;

namespace MyShop.Application.Abstractions;

public interface IReadOnlyRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id);

    Task<PaginatedResponse<TEntity>> ListAllAsync(
        int pageIndex,
        int pageSize);

    Task<PaginatedResponse<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        int pageIndex,
        int pageSize);
}
