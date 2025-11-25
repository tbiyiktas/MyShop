using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Non-generic entry point for IncludeBuilder configuration.
/// Holds the shared factory instance for all generic IncludeBuilder types.
/// </summary>
public static class IncludeBuilder
{
    private static IIncludeExpressionFactory? _factory;

    /// <summary>
    /// Initializes the IncludeBuilder system with the factory instance from DI.
    /// This method should be called once during application startup.
    /// </summary>
    /// <param name="factory">The factory for creating include expressions.</param>
    public static void Initialize(IIncludeExpressionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    internal static IIncludeExpressionFactory Factory
    {
        get
        {
            if (_factory == null)
                throw new InvalidOperationException(
                    "IncludeBuilder has not been initialized. Call IncludeBuilder.Initialize(factory) during application startup.");
            return _factory;
        }
    }
}

/// <summary>
/// Fluent builder for creating include expressions with support for ThenInclude.
/// Uses <see cref="IIncludeExpressionFactory"/> for creating include expression instances.
/// </summary>
public class IncludeBuilder<TEntity> where TEntity : class
{
    internal readonly List<IIncludeExpression<TEntity>> _includes = new();

    // Access the shared factory from the non-generic class
    private static IIncludeExpressionFactory Factory => IncludeBuilder.Factory;

    /// <summary>
    /// Adds an Include expression.
    /// </summary>
    public IncludeChain<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> expression)
    {
        var include = Factory.CreateSimpleInclude(expression);
        _includes.Add(include);
        return new IncludeChain<TEntity, TProperty>(this, include);
    }

    /// <summary>
    /// Adds a string-based Include expression.
    /// </summary>
    public IncludeBuilder<TEntity> Include(string includePath)
    {
        var include = Factory.CreateStringInclude<TEntity>(includePath);
        _includes.Add(include);
        return this;
    }

    internal List<IIncludeExpression<TEntity>> Build() => _includes;

    /// <summary>
    /// Creates a ThenInclude expression using the factory.
    /// </summary>
    internal static IIncludeExpression<TEntity> CreateThenInclude<TPreviousProperty, TProperty>(
        IIncludeExpression<TEntity> previousInclude,
        Expression<Func<TPreviousProperty, TProperty>> expression,
        bool isCollection)
    {
        return Factory.CreateThenInclude<TEntity, TPreviousProperty, TProperty>(
            previousInclude, expression, isCollection);
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
