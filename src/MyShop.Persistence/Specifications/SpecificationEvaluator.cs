using Microsoft.EntityFrameworkCore;
using MyShop.Application.Abstractions;

namespace MyShop.Persistence.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this IQueryable<TEntity> query,
        ISpecification<TEntity> specification)
        where TEntity : class
    {
        if (specification is null)
            return query;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        foreach (var includeExpression in specification.Includes)
        {
            query = query.Include(includeExpression);
        }

        if (specification.OrderBy is not null)
        {
            query = specification.OrderBy(query);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = specification.OrderByDescending(query);
        }

        return query;
    }
}
