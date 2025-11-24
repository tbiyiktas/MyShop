namespace MyShop.Domain.Common.FluentValidator;

public class RawErrorValidator : FluentValidatorBase
{
    public RawErrorValidator(IEnumerable<string> errors)
    {
        AddErrors(errors);
    }
}
