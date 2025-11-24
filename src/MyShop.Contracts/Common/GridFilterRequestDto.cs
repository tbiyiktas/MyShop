namespace MyShop.Contracts.Common;

/// <summary>
/// Gruplar arasındaki ilişki GroupOperator ile belirlenir.
/// Örn: (Group1) OR (Group2)
/// </summary>
public sealed class GridFilterRequestDto
{
    public List<FilterGroupDto> Groups { get; set; } = new();

    public FilterLogicalOperator GroupOperator { get; set; } = FilterLogicalOperator.And;

    public List<SortDto> Sorts { get; set; } = new();

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
