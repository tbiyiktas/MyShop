using MyShop.Domain.Common;
using MyShop.Domain.Common.FluentValidator;

namespace MyShop.Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    private Product() { }

    private Product(string name, decimal price, int stockQuantity, int categoryId, bool isActive = true)
    {

        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        IsActive = isActive;
    }

    // STATIC FACTORY
    public static Result<Product> Create(
        string name,
        decimal price,
        int stockQuantity,
        int categoryId,
        bool isActive = true)
    {
        var agg = FluentValidator.CreateAggregate()
            .Add(FluentValidator.ForString(name, nameof(Name))
                .NotEmpty()
                .NotWhitespace()
                .MinLength(3)
                .MaxLength(200))
            .Add(FluentValidator.ForDecimal(price, nameof(Price))
                .GreaterThanOrEqualTo(0))
            .Add(FluentValidator.ForInt(stockQuantity, nameof(StockQuantity))
                .GreaterThanOrEqualTo(0))
            .Add(FluentValidator.ForInt(categoryId, nameof(CategoryId))
                .GreaterThan(0));

        if (agg.HasError)
        {
            return Result<Product>.Failed(agg.Errors);
        }

        var product = new Product(name, price, stockQuantity, categoryId, isActive);
        return Result<Product>.Success(product);
    }
    public Result ChangePrice(decimal newPrice)
    {
        var validator = FluentValidator.ForDecimal(newPrice, nameof(Price))
                .GreaterThanOrEqualTo(0);

        if (validator.HasError)
        {
            return Result.Failed(validator.Errors);
        }

        Price = newPrice;
        return Result.Success();
    }

    public Result DecreaseStock(int quantity)
    {
        var agg = FluentValidator.CreateAggregate()
            .Add(FluentValidator.ForInt(quantity, nameof(quantity))
                .GreaterThan(0))
            .Add(FluentValidator.ForInt(StockQuantity, nameof(StockQuantity))
                .GreaterThanOrEqualTo(quantity));

        if (agg.HasError)
        {
            return Result.Failed(agg.Errors);
        }

        StockQuantity -= quantity;
        return Result.Success();
    }

    public Result IncreaseStock(int quantity)
    {
        var validator = FluentValidator.ForInt(quantity, nameof(quantity))
                .GreaterThan(0);

        if (validator.HasError)
        {
            return Result.Failed(validator.Errors);
        }

        StockQuantity += quantity;
        return Result.Success();
    }

    public Result Update(string name, decimal price, int stockQuantity, int categoryId)
    {
        var agg = FluentValidator.CreateAggregate()
            .Add(FluentValidator.ForString(name, nameof(Name))
                .NotEmpty()
                .NotWhitespace()
                .MinLength(3)
                .MaxLength(200))
            .Add(FluentValidator.ForDecimal(price, nameof(Price))
                .GreaterThanOrEqualTo(0))
            .Add(FluentValidator.ForInt(stockQuantity, nameof(StockQuantity))
                .GreaterThanOrEqualTo(0))
            .Add(FluentValidator.ForInt(categoryId, nameof(CategoryId))
                .GreaterThan(0));

        if (agg.HasError)
        {
            return Result.Failed(agg.Errors);
        }

        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;

        return Result.Success();
    }

    public Result Activate() { IsActive = true;  return Result.Success(); }
    public Result Deactivate()
    {
        IsActive = false; return Result.Success();
    }
}
