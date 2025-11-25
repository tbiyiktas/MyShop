using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Extension methods for composing specifications using logical operators.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two specifications using AND logic.
    /// The resulting specification matches entities that satisfy both the left and right specifications.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical AND of the two specifications.</returns>
    /// <remarks>
    /// <para>
    /// Criteria are combined using AND logic: <c>(left.Criteria) AND (right.Criteria)</c>.
    /// </para>
    /// <para>
    /// Include expressions from both specifications are merged and deduplicated.
    /// </para>
    /// <para>
    /// Query flags (AsNoTracking, IgnoreQueryFilters) are set to true if either specification has them enabled.
    /// </para>
    /// <example>
    /// <code>
    /// var lowStock = new LowStockSpecification(threshold: 10);
    /// var expensive = new ExpensiveProductSpecification(minPrice: 100);
    /// var spec = lowStock.And(expensive);
    /// // Returns products with stock &lt; 10 AND price &gt;= 100
    /// </code>
    /// </example>
    /// </remarks>
    public static ISpecification<TEntity> And<TEntity>(
        this ISpecification<TEntity> left,
        ISpecification<TEntity> right)
        where TEntity : class
        => new AndSpecification<TEntity>(left, right);

    /// <summary>
    /// Combines two specifications using OR logic.
    /// The resulting specification matches entities that satisfy either the left or right specification.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical OR of the two specifications.</returns>
    /// <remarks>
    /// <para>
    /// Criteria are combined using OR logic: <c>(left.Criteria) OR (right.Criteria)</c>.
    /// </para>
    /// <para>
    /// Include expressions from both specifications are merged and deduplicated.
    /// </para>
    /// <para>
    /// Query flags (AsNoTracking, IgnoreQueryFilters) are set to true if either specification has them enabled.
    /// </para>
    /// <example>
    /// <code>
    /// var lowStock = new LowStockSpecification(threshold: 5);
    /// var discontinued = new DiscontinuedProductSpecification();
    /// var spec = lowStock.Or(discontinued);
    /// // Returns products with stock &lt; 5 OR discontinued products
    /// </code>
    /// </example>
    /// </remarks>
    public static ISpecification<TEntity> Or<TEntity>(
        this ISpecification<TEntity> left,
        ISpecification<TEntity> right)
        where TEntity : class
        => new OrSpecification<TEntity>(left, right);

    /// <summary>
    /// Negates a specification using NOT logic.
    /// The resulting specification matches entities that do NOT satisfy the original specification.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="specification">The specification to negate.</param>
    /// <returns>A new specification that represents the logical NOT of the original specification.</returns>
    /// <remarks>
    /// <para>
    /// The criteria is negated: <c>NOT (specification.Criteria)</c>.
    /// </para>
    /// <para>
    /// Include expressions, ordering, and query flags are preserved from the original specification.
    /// </para>
    /// <example>
    /// <code>
    /// var activeProducts = new ActiveProductSpecification();
    /// var inactiveProducts = activeProducts.Not();
    /// // Returns products where IsActive = false
    /// </code>
    /// </example>
    /// </remarks>
    public static ISpecification<TEntity> Not<TEntity>(
        this ISpecification<TEntity> specification)
        where TEntity : class
        => new NotSpecification<TEntity>(specification);
}
