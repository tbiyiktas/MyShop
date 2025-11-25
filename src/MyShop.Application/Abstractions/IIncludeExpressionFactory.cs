using System.Linq.Expressions;

namespace MyShop.Application.Abstractions;

/// <summary>
/// Factory for creating include expression instances.
/// This abstraction allows the Application layer to remain independent of the Persistence layer implementation.
/// </summary>
public interface IIncludeExpressionFactory
{
    /// <summary>
    /// Creates a simple include expression for a navigation property.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TProperty">The navigation property type.</typeparam>
    /// <param name="expression">Expression selecting the navigation property.</param>
    /// <returns>An include expression that can be applied to a query.</returns>
    IIncludeExpression<TEntity> CreateSimpleInclude<TEntity, TProperty>(
        Expression<Func<TEntity, TProperty>> expression)
        where TEntity : class;

    /// <summary>
    /// Creates a string-based include expression.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="includePath">The navigation property path as a string.</param>
    /// <returns>An include expression that can be applied to a query.</returns>
    IIncludeExpression<TEntity> CreateStringInclude<TEntity>(
        string includePath)
        where TEntity : class;

    /// <summary>
    /// Creates a then-include expression for chaining navigation properties.
    /// </summary>
    /// <typeparam name="TEntity">The root entity type.</typeparam>
    /// <typeparam name="TPreviousProperty">The type of the previously included property.</typeparam>
    /// <typeparam name="TProperty">The type of the property to include from the previous property.</typeparam>
    /// <param name="previousInclude">The previous include expression in the chain.</param>
    /// <param name="expression">Expression selecting the next navigation property.</param>
    /// <param name="isCollection">Whether the previous property is a collection.</param>
    /// <returns>An include expression that can be applied to a query.</returns>
    IIncludeExpression<TEntity> CreateThenInclude<TEntity, TPreviousProperty, TProperty>(
        IIncludeExpression<TEntity> previousInclude,
        Expression<Func<TPreviousProperty, TProperty>> expression,
        bool isCollection)
        where TEntity : class;
}
