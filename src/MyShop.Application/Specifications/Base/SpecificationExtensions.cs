using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

public static class SpecificationExtensions
{
    public static ISpecification<TEntity> And<TEntity>(
        this ISpecification<TEntity> left,
        ISpecification<TEntity> right)
        where TEntity : class
        => new AndSpecification<TEntity>(left, right);

    public static ISpecification<TEntity> Or<TEntity>(
        this ISpecification<TEntity> left,
        ISpecification<TEntity> right)
        where TEntity : class
        => new OrSpecification<TEntity>(left, right);

    public static ISpecification<TEntity> Not<TEntity>(
        this ISpecification<TEntity> specification)
        where TEntity : class
        => new NotSpecification<TEntity>(specification);
}
