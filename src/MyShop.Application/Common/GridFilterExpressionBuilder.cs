using System.Linq.Expressions;
using MyShop.Application.Specifications.Base;
using MyShop.Contracts.Common;

namespace MyShop.Application.Common;

public static class GridFilterExpressionBuilder
{
    /// <summary>
    /// GridFilterRequestDto içinden (A AND B) OR (C AND D) gibi
    /// çok seviyeli bir predicate üretir.
    /// </summary>
    public static Expression<Func<T, bool>> BuildPredicate<T>(GridFilterRequestDto request)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        if (request.Groups is null || request.Groups.Count == 0)
        {
            // Filtre yoksa TRUE dönen predicate
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);
        }

        // 1) Her grup için ayrı predicate
        var groupPredicates = new List<Expression<Func<T, bool>>>();

        foreach (var group in request.Groups)
        {
            var groupExpr = BuildGroupPredicate<T>(parameter, group);
            if (groupExpr is not null)
            {
                groupPredicates.Add(groupExpr);
            }
        }

        if (groupPredicates.Count == 0)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);
        }

        // 2) Grupları GroupOperator ile birleştir: AND / OR
        var combined = groupPredicates[0];

        for (int i = 1; i < groupPredicates.Count; i++)
        {
            combined = request.GroupOperator switch
            {
                FilterLogicalOperator.And =>
                    ExpressionCombiner.And(combined, groupPredicates[i]),
                FilterLogicalOperator.Or =>
                    ExpressionCombiner.Or(combined, groupPredicates[i]),
                _ => combined
            };
        }

        return combined;
    }

    private static Expression<Func<T, bool>>? BuildGroupPredicate<T>(
        ParameterExpression parameter,
        FilterGroupDto group)
    {
        if (group.Conditions is null || group.Conditions.Count == 0)
            return null;

        // Her condition için tek predicate üret
        var conditionPredicates = new List<Expression<Func<T, bool>>>();

        foreach (var condition in group.Conditions)
        {
            var criterion = condition.ToFilterCriterion();

            // Tek kriterlik bir listeyi mevcut ExpressionBuilder'a verelim
            var single = ExpressionBuilder.BuildAndPredicate<T>(new[] { criterion });
            conditionPredicates.Add(single);
        }

        if (conditionPredicates.Count == 0)
            return null;

        var combined = conditionPredicates[0];

        for (int i = 1; i < conditionPredicates.Count; i++)
        {
            combined = group.Operator switch
            {
                FilterLogicalOperator.And =>
                    ExpressionCombiner.And(combined, conditionPredicates[i]),
                FilterLogicalOperator.Or =>
                    ExpressionCombiner.Or(combined, conditionPredicates[i]),
                _ => combined
            };
        }

        return combined;
    }
}
