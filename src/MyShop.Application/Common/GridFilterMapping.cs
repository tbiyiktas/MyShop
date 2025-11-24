using MyShop.Contracts.Common;

namespace MyShop.Application.Common;

public static class GridFilterMapping
{
    public static FilterCriterion ToFilterCriterion(this FilterConditionDto dto)
        => new(
            propertyPath: dto.PropertyPath,
            operation: (MyShop.Contracts.Common.FilterOperation)dto.Operation,
            value: dto.Value,
            caseInsensitive: dto.CaseInsensitive);

    public static SortCriterion ToSortCriterion(this SortDto dto)
        => new(dto.PropertyPath, dto.Descending);

    //public static IReadOnlyList<SortCriterion> ToSortCriteria(this IEnumerable<SortDto> dtos)
    //    => dtos?.Select(d => d.ToSortCriterion()).ToList()
    //       ?? Array.Empty<SortCriterion>();

    public static IReadOnlyList<SortCriterion> ToSortCriteria(this IEnumerable<SortDto>? dtos)
    {
        if (dtos is null)
            return Array.Empty<SortCriterion>();

        return dtos.Select(d => d.ToSortCriterion()).ToList();
    }
}