using System.Linq.Expressions;

namespace MyShop.Application.Specifications.Base;

public static class ExpressionCombiner
{
    public static Expression<Func<T, bool>> And<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));

        var parameter = left.Parameters.Single();
        var rightBody = new ParameterReplacer(right.Parameters.Single(), parameter).Visit(right.Body);
        var body = Expression.AndAlso(left.Body, rightBody!);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> Or<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));

        var parameter = left.Parameters.Single();
        var rightBody = new ParameterReplacer(right.Parameters.Single(), parameter).Visit(right.Body);
        var body = Expression.OrElse(left.Body, rightBody!);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
