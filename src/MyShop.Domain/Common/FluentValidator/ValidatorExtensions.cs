namespace MyShop.Domain.Common.FluentValidator;

public static class ValidatorExtensions
{
    // Genel bir doğrulayıcıdan Result nesnesi oluşturur
    public static Result ToResult(this FluentValidatorBase validator)
    {
        if (validator.HasError)
        {
            // Use ValidationErrors to preserve rich error details
            return Result.Failed(validator.ValidationErrors);
        }
        return Result.Success();
    }

    //Değer içeren bir doğrulayıcıdan Result<T> nesnesi oluşturur
    public static Result<T> ToResult<T>(this FluentValidatorBase<T> validator)
    {
        if (validator.HasError)
        {
            return Result<T>.Failed(validator.ValidationErrors);
        }
        // Başarılı durumda, doğrulanmış değeri Result<T> içine koyarız
        return Result<T>.Success(validator.Value);
    }
}
