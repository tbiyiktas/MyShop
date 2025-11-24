namespace MyShop.Domain.Common.FluentValidator;

public class FluentValidatorAggregate : FluentValidatorBase
{
    private readonly List<FluentValidatorBase> _validators = new();

    public static FluentValidatorAggregate Create() => new();

    public FluentValidatorAggregate Add(FluentValidatorBase validator)
    {
        if (validator.HasError)
            _validators.Add(validator);
        return this;
    }

    public FluentValidatorAggregate Add(Result result)
    {
        if (result.HasError && result.Errors != null)
        {
            _validators.Add(new RawErrorValidator(result.Errors));
        }
        return this;
    }

    public FluentValidatorAggregate Add<T>(Result<T> result)
    {
        if (result.HasError && result.Errors != null)
        {
            _validators.Add(new RawErrorValidator(result.Errors));
        }

        return this;
    }

    public override bool HasError => _validators.Any(v => v.HasError);

    public override List<string> Errors => _validators.SelectMany(v => v.Errors).ToList();

    public override List<ValidationError> ValidationErrors => _validators.SelectMany(v => v.ValidationErrors).ToList();
}
