using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

public abstract class Specification<TEntity> : ISpecification<TEntity>
    where TEntity : class
{
    public virtual Expression<Func<TEntity, bool>>? Criteria { get; protected set; }

    public List<Expression<Func<TEntity, object>>> Includes { get; }
        = new();

    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; protected set; }
    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderByDescending { get; protected set; }

    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
        => Includes.Add(includeExpression);

    protected void ApplyOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        => OrderBy = orderByExpression;

    protected void ApplyOrderByDescending(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByDescendingExpression)
        => OrderByDescending = orderByDescendingExpression;
}
