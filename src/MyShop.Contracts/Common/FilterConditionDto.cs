namespace MyShop.Contracts.Common;

/// <summary>
/// Tek bir kolon üzerindeki tek bir şart (ör: Price &gt;= 1000).
/// </summary>
public sealed class FilterConditionDto
{
    // "Price", "Name", "Category.Name" gibi (nested path dahil)
    public string PropertyPath { get; set; } = null!;

    public FilterOperation Operation { get; set; }

    // Her zaman string gelir, backend tarafında tipine convert edeceğiz.
    public string? Value { get; set; }

    public bool CaseInsensitive { get; set; }
}
