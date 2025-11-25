using MyShop.Application.Abstractions;
using System.Linq.Expressions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Represents the logical NOT (negation) of a specification.
/// Inverts the criteria while preserving include expressions, ordering, and query flags.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// <para>
/// This specification is created by the <see cref="SpecificationExtensions.Not{TEntity}"/> extension method.
/// </para>
/// <para>
/// <strong>Criteria Negation:</strong> The criteria expression is negated using <c>Expression.Not</c>.
/// If the inner specification has no criteria, the result also has no criteria.
/// </para>
/// <para>
/// <strong>Preservation:</strong> Include expressions, ordering, and query flags (AsNoTracking, IgnoreQueryFilters)
/// are preserved from the inner specification without modification.
/// </para>
/// </remarks>
public sealed class NotSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotSpecification{TEntity}"/> class.
    /// </summary>
    /// <param name="inner">The specification to negate.</param>
    public NotSpecification(ISpecification<TEntity> inner)
    {
        if (inner.Criteria is null)
        {
            Criteria = null;
        }
        else
        {
            var param = inner.Criteria.Parameters.Single();
            var body = Expression.Not(inner.Criteria.Body);
            Criteria = Expression.Lambda<Func<TEntity, bool>>(body, param);
        }

        // Includes & Order'ı mirror et
        IncludeExpressions.AddRange(inner.IncludeExpressions);
        OrderBy = inner.OrderBy;
        OrderByDescending = inner.OrderByDescending;

        // AsNoTracking & IgnoreQueryFilters & AsSplitQuery'ı mirror et
        if (inner.AsNoTracking)
        {
            ApplyAsNoTracking();
        }

        if (inner.IgnoreQueryFilters)
        {
            ApplyIgnoreQueryFilters();
        }

        if (inner.AsSplitQuery)
        {
            ApplyAsSplitQuery();
        }
    }
}
