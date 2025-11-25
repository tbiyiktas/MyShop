using MyShop.Application.Specifications.Base;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MyShop.Application.Tests;

/// <summary>
/// Helper specification used in unit tests to expose the protected configuration methods of <see cref="Specification{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class TestSpecification<TEntity> : Specification<TEntity> where TEntity : class
{
    public TestSpecification() { }

    public void Include(Action<IncludeBuilder<TEntity>> configure) => ApplyInclude(configure);
    public void SetAsNoTracking() => ApplyAsNoTracking();
    public void SetAsSplitQuery() => ApplyAsSplitQuery();
    public void IgnoreFilters() => ApplyIgnoreQueryFilters();
    public void SetOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy) => ApplyOrderBy(orderBy);
    public void SetOrderByDescending(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByDesc) => ApplyOrderByDescending(orderByDesc);
    public void ThenBy(Expression<Func<TEntity, object>> thenBy) => ApplyThenBy(thenBy);
    public void ThenByDescending(Expression<Func<TEntity, object>> thenByDesc) => ApplyThenByDescending(thenByDesc);
}
