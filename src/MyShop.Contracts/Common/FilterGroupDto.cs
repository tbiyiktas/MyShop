namespace MyShop.Contracts.Common;

/// <summary>
/// Bir grup içindeki şartlar; aralarındaki ilişki Operator ile belirlenir.
/// Örn: (A AND B) veya (C OR D)
/// </summary>
public sealed class FilterGroupDto
{
    public FilterLogicalOperator Operator { get; set; } = FilterLogicalOperator.And;

    public List<FilterConditionDto> Conditions { get; set; } = new();
}
