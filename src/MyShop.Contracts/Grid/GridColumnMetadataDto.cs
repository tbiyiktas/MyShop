using MyShop.Contracts.Common;

namespace MyShop.Contracts.Grid;

public sealed class GridColumnMetadataDto
{
    // "Name", "Price", "Category.Name" vb.
    public string PropertyPath { get; set; } = null!;

    // UI'de gösterilecek ad
    public string DisplayName { get; set; } = null!;

    public GridColumnType ColumnType { get; set; }

    public bool Filterable { get; set; } = true;
    public bool Sortable { get; set; } = true;

    // Bu kolon için izin verilen FilterOperation'lar (enum int olarak gider gelir)
    public List<FilterOperation> AllowedOperations { get; set; } = new();

    // UI'da kullanılacak input tipi: "Text", "Number", "Date", "Boolean", "Dropdown" vb.
    public string? InputType { get; set; }

    // Dropdown ise verinin çekileceği URL
    public string? LookupUrl { get; set; }

    // Dropdown'dan seçilen value hangi property'ye denk geliyor? (örn: "id")
    public string? LookupValueField { get; set; }

    // Dropdown'da görünen text hangi property? (örn: "name")
    public string? LookupTextField { get; set; }
}
