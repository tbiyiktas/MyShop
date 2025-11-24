namespace MyShop.Contracts.Common;

public sealed class SortDto
{
    public string PropertyPath { get; set; } = null!;
    public bool Descending { get; set; }
}
