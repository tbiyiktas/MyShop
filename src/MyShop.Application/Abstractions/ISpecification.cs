using System.Linq.Expressions;

namespace MyShop.Application.Abstractions;

public interface ISpecification<TEntity> where TEntity : class
{
    Expression<Func<TEntity, bool>>? Criteria { get; }

    List<Expression<Func<TEntity, object>>> Includes { get; }

    // Sıralama için; IQueryable'ı sızdırmadan içerde kullanacağız.
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; }
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderByDescending { get; }
}
