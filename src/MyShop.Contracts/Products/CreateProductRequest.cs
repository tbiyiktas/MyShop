namespace MyShop.Contracts.Products;

public sealed class CreateProductRequest
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int CategoryId { get; init; }
}
