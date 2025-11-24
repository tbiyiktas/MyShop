using MyShop.Contracts.Common;

namespace MyShop.Application.Common;

public sealed class FilterCriterion
{
    public string PropertyPath { get; }
    public FilterOperation Operation { get; }
    public object? Value { get; }
    public bool CaseInsensitive { get; }

    public FilterCriterion(
        string propertyPath,
        FilterOperation operation,
        object? value,
        bool caseInsensitive = false)
    {
        PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
        Operation = operation;
        Value = value;
        CaseInsensitive = caseInsensitive;
    }
}