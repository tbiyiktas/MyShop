using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Fluent builder for creating include expressions with ThenInclude support.
/// Note: Actual EF Core implementation is in Persistence layer.
/// </summary>
public class IncludeBuilder<TEntity> where TEntity : class
{
    internal readonly List<IIncludeExpression<TEntity>> _includes = new();

    /// <summary>
    /// Adds an Include expression.
    /// </summary>
    public IncludeChain<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> expression)
    {
        // Create include expression - implementation will be in Persistence layer
        var include = CreateSimpleInclude(expression);
        _includes.Add(include);
        return new IncludeChain<TEntity, TProperty>(this, include);
    }

    /// <summary>
    /// Adds a string-based Include expression.
    /// </summary>
    public IncludeBuilder<TEntity> Include(string includePath)
    {
        var include = CreateStringInclude(includePath);
        _includes.Add(include);
        return this;
    }

    internal List<IIncludeExpression<TEntity>> Build() => _includes;

    // Factory methods - implementations provided by Persistence layer via reflection/factory
    private static IIncludeExpression<TEntity> CreateSimpleInclude<TProperty>(
        Expression<Func<TEntity, TProperty>> expression)
    {
        // Use reflection to create instance from Persistence layer
        var type = Type.GetType("MyShop.Persistence.Specifications.SimpleIncludeExpression`2, MyShop.Persistence");
        if (type == null)
            throw new InvalidOperationException("SimpleIncludeExpression not found in Persistence layer");

        var genericType = type.MakeGenericType(typeof(TEntity), typeof(TProperty));
        return (IIncludeExpression<TEntity>)Activator.CreateInstance(genericType, expression)!;
    }

    private static IIncludeExpression<TEntity> CreateStringInclude(string includePath)
    {
        var type = Type.GetType("MyShop.Persistence.Specifications.StringIncludeExpression`1, MyShop.Persistence");
        if (type == null)
            throw new InvalidOperationException("StringIncludeExpression not found in Persistence layer");

        var genericType = type.MakeGenericType(typeof(TEntity));
        return (IIncludeExpression<TEntity>)Activator.CreateInstance(genericType, includePath)!;
    }

    internal static IIncludeExpression<TEntity> CreateThenInclude<TPreviousProperty, TProperty>(
        IIncludeExpression<TEntity> previousInclude,
        Expression<Func<TPreviousProperty, TProperty>> expression,
        bool isCollection)
    {
        var type = Type.GetType("MyShop.Persistence.Specifications.ThenIncludeExpression`3, MyShop.Persistence");
        if (type == null)
            throw new InvalidOperationException("ThenIncludeExpression not found in Persistence layer");

        var genericType = type.MakeGenericType(typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty));
        return (IIncludeExpression<TEntity>)Activator.CreateInstance(genericType, previousInclude, expression, isCollection)!;
    }
}

/// <summary>
/// Represents a chain of includes that can be extended with ThenInclude.
/// </summary>
public class IncludeChain<TEntity, TPreviousProperty> where TEntity : class
{
    private readonly IncludeBuilder<TEntity> _root;
    private readonly IIncludeExpression<TEntity> _previousInclude;

    internal IncludeChain(IncludeBuilder<TEntity> root, IIncludeExpression<TEntity> previousInclude)
    {
        _root = root;
        _previousInclude = previousInclude;
    }

    /// <summary>
    /// Adds a ThenInclude for a single navigation property.
    /// </summary>
    public IncludeChain<TEntity, TProperty> ThenInclude<TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> expression)
    {
        var include = IncludeBuilder<TEntity>.CreateThenInclude<TPreviousProperty, TProperty>(
            _previousInclude, expression, isCollection: false);
        _root._includes.Add(include);
        return new IncludeChain<TEntity, TProperty>(_root, include);
    }

    /// <summary>
    /// Adds a new Include expression (starts a new chain).
    /// </summary>
    public IncludeChain<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> expression)
    {
        return _root.Include(expression);
    }

    /// <summary>
    /// Adds a string-based Include expression.
    /// </summary>
    public IncludeBuilder<TEntity> Include(string includePath)
    {
        return _root.Include(includePath);
    }
}
