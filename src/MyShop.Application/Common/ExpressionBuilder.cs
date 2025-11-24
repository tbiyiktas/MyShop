using MyShop.Contracts.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace MyShop.Application.Common;

public static class ExpressionBuilder
{
    // ---------------- FILTER ----------------

    public static Expression<Func<T, bool>> BuildAndPredicate<T>(
        IEnumerable<FilterCriterion> filters)
    {
        var filterList = filters?.ToList() ?? new List<FilterCriterion>();
        var parameter = Expression.Parameter(typeof(T), "x");

        Expression? body = null;

        foreach (var filter in filterList)
        {
            var expr = BuildSinglePredicate<T>(parameter, filter);
            if (expr is null)
                continue;

            body = body is null
                ? expr
                : Expression.AndAlso(body, expr);
        }

        if (body is null)
        {
            body = Expression.Constant(true);
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression? BuildSinglePredicate<T>(
        ParameterExpression parameter,
        FilterCriterion filter)
    {
        // Nested path destekli: "Category.Name" vs.
        var left = BuildPropertyPathExpression(parameter, filter.PropertyPath);
        var memberType = left.Type;

        var nonNullableType = Nullable.GetUnderlyingType(memberType) ?? memberType;

        object? convertedValue = null;
        if (filter.Value is not null)
        {
            convertedValue = Convert.ChangeType(filter.Value, nonNullableType);
        }

        Expression right = Expression.Constant(convertedValue, nonNullableType);

        // constant'ı member tipine cast et (nullable vs.)
        if (memberType != nonNullableType)
        {
            right = Expression.Convert(right, memberType);
        }

        // string özel
        if (memberType == typeof(string))
        {
            return BuildStringPredicate(left, right, filter);
        }

        return filter.Operation switch
        {
            FilterOperation.Equals =>
                Expression.Equal(left, right),

            FilterOperation.NotEquals =>
                Expression.NotEqual(left, right),

            FilterOperation.GreaterThan =>
                Expression.GreaterThan(left, right),

            FilterOperation.GreaterThanOrEqual =>
                Expression.GreaterThanOrEqual(left, right),

            FilterOperation.LessThan =>
                Expression.LessThan(left, right),

            FilterOperation.LessThanOrEqual =>
                Expression.LessThanOrEqual(left, right),

            _ => throw new NotSupportedException(
                $"Operation '{filter.Operation}' is not supported for type '{memberType.Name}'.")
        };
    }

    private static Expression BuildStringPredicate(
        Expression left,
        Expression right,
        FilterCriterion filter)
    {
        Expression leftExpr = left;
        Expression rightExpr = right;

        if (filter.CaseInsensitive)
        {
            var toLower = typeof(string).GetMethod(
                nameof(string.ToLower),
                Type.EmptyTypes)!;

            leftExpr = Expression.Call(leftExpr, toLower);
            rightExpr = Expression.Call(rightExpr, toLower);
        }

        return filter.Operation switch
        {
            FilterOperation.Equals =>
                Expression.Equal(leftExpr, rightExpr),

            FilterOperation.NotEquals =>
                Expression.NotEqual(leftExpr, rightExpr),

            FilterOperation.Contains =>
                Expression.Call(leftExpr,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    rightExpr),

            FilterOperation.StartsWith =>
                Expression.Call(leftExpr,
                    nameof(string.StartsWith),
                    Type.EmptyTypes,
                    rightExpr),

            FilterOperation.EndsWith =>
                Expression.Call(leftExpr,
                    nameof(string.EndsWith),
                    Type.EmptyTypes,
                    rightExpr),

            _ => throw new NotSupportedException(
                $"Operation '{filter.Operation}' is not supported for string.")
        };
    }

    /// <summary>
    /// "Category.Name" gibi property path'leri destekler.
    /// x =&gt; x.Category.Name
    /// </summary>
    private static Expression BuildPropertyPathExpression(
        Expression root,
        string propertyPath)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        if (string.IsNullOrWhiteSpace(propertyPath))
            throw new ArgumentNullException(nameof(propertyPath));

        var parts = propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Expression current = root;
        foreach (var part in parts)
        {
            current = Expression.PropertyOrField(current, part);
        }

        return current;
    }

    // ---------------- SORTING ----------------

    /// <summary>
    /// Tek alan sort: Specification.ApplyOrderBy ile kullanmak için
    /// Func&lt;IQueryable&lt;T&gt;, IOrderedQueryable&lt;T&gt;&gt; döndürür.
    /// </summary>
    public static Func<IQueryable<T>, IOrderedQueryable<T>> BuildOrderBy<T>(
        SortCriterion sort)
    {
        if (sort is null) throw new ArgumentNullException(nameof(sort));

        return query => query.OrderByPropertyPath(sort.PropertyPath, sort.Descending);
    }

    /// <summary>
    /// Çoklu sort: OrderBy + ThenBy(…) zinciri.
    /// </summary>
    public static Func<IQueryable<T>, IOrderedQueryable<T>> BuildOrderBy<T>(
        IReadOnlyList<SortCriterion> sorts)
    {
        if (sorts is null) throw new ArgumentNullException(nameof(sorts));
        if (sorts.Count == 0)
            throw new ArgumentException("At least one sort criterion is required.", nameof(sorts));

        return query =>
        {
            IOrderedQueryable<T>? ordered = null;
            IQueryable<T> current = query;

            for (var i = 0; i < sorts.Count; i++)
            {
                var s = sorts[i];

                if (i == 0)
                {
                    ordered = current.OrderByPropertyPath(s.PropertyPath, s.Descending);
                }
                else
                {
                    ordered = ordered!.ThenByPropertyPath(s.PropertyPath, s.Descending);
                }
            }

            return ordered!;
        };
    }

    public static IOrderedQueryable<T> OrderByPropertyPath<T>(
        this IQueryable<T> source,
        string propertyPath,
        bool descending)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(propertyPath))
            throw new ArgumentNullException(nameof(propertyPath));

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = BuildPropertyPathExpression(parameter, propertyPath);
        var keySelector = Expression.Lambda(body, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";

        return (IOrderedQueryable<T>)CallOrderMethod(
            source,
            methodName,
            typeof(T),
            body.Type,
            keySelector);
    }

    public static IOrderedQueryable<T> ThenByPropertyPath<T>(
        this IOrderedQueryable<T> source,
        string propertyPath,
        bool descending)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(propertyPath))
            throw new ArgumentNullException(nameof(propertyPath));

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = BuildPropertyPathExpression(parameter, propertyPath);
        var keySelector = Expression.Lambda(body, parameter);

        var methodName = descending ? "ThenByDescending" : "ThenBy";

        return (IOrderedQueryable<T>)CallOrderMethod(
            source,
            methodName,
            typeof(T),
            body.Type,
            keySelector);
    }

    private static object CallOrderMethod(
        IQueryable source,
        string methodName,
        Type entityType,
        Type keyType,
        LambdaExpression keySelector)
    {
        var method = typeof(Queryable).GetMethods()
            .First(m =>
                m.Name == methodName &&
                m.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(entityType, keyType);

        return genericMethod.Invoke(null, new object[] { source, keySelector })!;
    }
}
