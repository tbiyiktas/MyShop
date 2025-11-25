using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Persistence.Specifications;

/// <summary>
/// Factory implementation for creating include expression instances used by the specification pattern.
/// This class lives in the Persistence layer and provides EF Core specific include expressions.
/// </summary>
public class IncludeExpressionFactory : IIncludeExpressionFactory
{
    /// <inheritdoc />
    public IIncludeExpression<TEntity> CreateSimpleInclude<TEntity, TProperty>(
        Expression<Func<TEntity, TProperty>> expression)
        where TEntity : class
    {
        return new SimpleIncludeExpression<TEntity, TProperty>(expression);
    }

    /// <inheritdoc />
    public IIncludeExpression<TEntity> CreateStringInclude<TEntity>(
        string includePath)
        where TEntity : class
    {
        return new StringIncludeExpression<TEntity>(includePath);
    }

    /// <inheritdoc />
    public IIncludeExpression<TEntity> CreateThenInclude<TEntity, TPreviousProperty, TProperty>(
        IIncludeExpression<TEntity> previousInclude,
        Expression<Func<TPreviousProperty, TProperty>> expression,
        bool isCollection)
        where TEntity : class
    {
        return new ThenIncludeExpression<TEntity, TPreviousProperty, TProperty>(
            previousInclude, expression, isCollection);
    }
}
