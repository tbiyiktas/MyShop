namespace MyShop.Contracts.Products;

public sealed class ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public int CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
