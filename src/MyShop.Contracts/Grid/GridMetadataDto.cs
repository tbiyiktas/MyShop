namespace MyShop.Contracts.Grid;

public sealed class GridMetadataDto
{
    public string EntityName { get; set; } = null!;
    public List<GridColumnMetadataDto> Columns { get; set; } = new();
}
