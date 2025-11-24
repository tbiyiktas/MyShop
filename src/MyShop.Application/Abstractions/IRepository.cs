namespace MyShop.Application.Abstractions;
public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
    where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
