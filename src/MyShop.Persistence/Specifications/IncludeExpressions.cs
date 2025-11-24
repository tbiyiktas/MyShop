using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MyShop.Application.Abstractions;

namespace MyShop.Persistence.Specifications;

/// <summary>
/// Simple include expression (e.g., Include(p => p.Category))
/// </summary>
public class SimpleIncludeExpression<TEntity, TProperty> : IIncludeExpression<TEntity>
    where TEntity : class
{
    private readonly Expression<Func<TEntity, TProperty>> _expression;

    public SimpleIncludeExpression(Expression<Func<TEntity, TProperty>> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        return query.Include(_expression);
    }
}

/// <summary>
/// ThenInclude expression (e.g., ThenInclude(c => c.ParentCategory))
/// </summary>
public class ThenIncludeExpression<TEntity, TPreviousProperty, TProperty> : IIncludeExpression<TEntity>
    where TEntity : class
{
    private readonly IIncludeExpression<TEntity> _previousInclude;
    private readonly Expression<Func<TPreviousProperty, TProperty>> _expression;
    private readonly bool _isCollection;

    public ThenIncludeExpression(
        IIncludeExpression<TEntity> previousInclude,
        Expression<Func<TPreviousProperty, TProperty>> expression,
        bool isCollection = false)
    {
        _previousInclude = previousInclude ?? throw new ArgumentNullException(nameof(previousInclude));
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        _isCollection = isCollection;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        // Apply previous include first
        var includedQuery = _previousInclude.Apply(query);

        // Cast to IIncludableQueryable to enable ThenInclude
        if (_isCollection)
        {
            var includableQuery = (IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>>)includedQuery;
            return includableQuery.ThenInclude(_expression);
        }
        else
        {
            var includableQuery = (IIncludableQueryable<TEntity, TPreviousProperty>)includedQuery;
            return includableQuery.ThenInclude(_expression);
        }
    }
}

/// <summary>
/// String-based include expression (e.g., Include("Category.ParentCategory"))
/// </summary>
public class StringIncludeExpression<TEntity> : IIncludeExpression<TEntity>
    where TEntity : class
{
    private readonly string _includePath;

    public StringIncludeExpression(string includePath)
    {
        if (string.IsNullOrWhiteSpace(includePath))
            throw new ArgumentException("Include path cannot be null or empty", nameof(includePath));

        _includePath = includePath;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        return query.Include(_includePath);
    }
}
