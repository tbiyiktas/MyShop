using System.Linq.Expressions;

namespace MyShop.Application.Specifications.Base;

public sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;
    private readonly ParameterExpression _replacement;

    public ParameterReplacer(ParameterExpression parameter, ParameterExpression replacement)
    {
        _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        _replacement = replacement ?? throw new ArgumentNullException(nameof(replacement));
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _parameter ? _replacement : base.VisitParameter(node);
}
