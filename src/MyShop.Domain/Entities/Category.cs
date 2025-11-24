using MyShop.Domain.Common;
using MyShop.Domain.Common.FluentValidator;

namespace MyShop.Domain.Entities;

public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;

    private Category() { }

    private Category(string name)
    {
        Name = name;
    }

    // STATIC FACTORY
    public static Result<Category> Create(string name)
    {
        var validator = FluentValidator.ForString(name, nameof(Name))
            .NotEmpty()
            .NotWhitespace()
            .MinLength(3)
            .MaxLength(100);

        if (validator.HasError)
        {
            return Result<Category>.Failed(validator.ValidationErrors);
        }

        var category = new Category(name);
        return Result<Category>.Success(category);
    }

    public Result ChangeName(string newName)
    {
        var validator = FluentValidator.ForString(newName, nameof(Name))
            .NotEmpty()
            .NotWhitespace()
            .MinLength(3)
            .MaxLength(100);

        if (validator.HasError)
        {
            return Result.Failed(validator.ValidationErrors);
        }

        Name = newName;
        return Result.Success();
    }
}
