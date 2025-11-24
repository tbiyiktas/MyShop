using Microsoft.EntityFrameworkCore;
using MyShop.Application.Abstractions;
using MyShop.Application.Common;
using MyShop.Contracts.Common;
using MyShop.Persistence.Specifications;

namespace MyShop.Persistence.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly DbContext DbContext;
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public EfRepository(DbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<PaginatedResponse<TEntity>> ListAllAsync(
        int pageIndex,
        int pageSize)
    {
        if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = DbSet.AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResponse<TEntity>.Create(items, totalCount, pageIndex, pageSize);
    }

    public virtual async Task<PaginatedResponse<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        int pageIndex,
        int pageSize)
    {
        if (specification is null) throw new ArgumentNullException(nameof(specification));
        if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        // 1) Filtre/Include/Order uygulanmış base query
        var baseQuery = DbSet.AsQueryable()
            .ApplySpecification(specification);

        var totalCount = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResponse<TEntity>.Create(items, totalCount, pageIndex, pageSize);
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
