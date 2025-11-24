namespace MyShop.Domain.Common.FluentValidator;

public abstract class FluentValidatorBase
{
    protected string? _nextMessage;

    protected readonly List<string> _errors = new();
    protected readonly List<ValidationError> _validationErrors = new();

    public virtual bool HasError => _errors.Any(); 
    public virtual List<string> Errors => _errors;
    public virtual List<ValidationError> ValidationErrors => _validationErrors;

    public void AddError(string errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            _errors.Add(errorMessage);
            // Create a generic validation error for string-only errors
            _validationErrors.Add(new ValidationError("General", errorMessage));
        }
    }

    public void AddError(ValidationError error)
    {
        _errors.Add(error.ToString());
        _validationErrors.Add(error);
    }

    protected void AddErrors(IEnumerable<string> errorMessages)
    {
        if (errorMessages != null)
        {
            foreach (var error in errorMessages)
            {
                AddError(error); 
            }
        }
    }

    public void ValidateAndAddError(bool condition, string errorMessage)
    {
        if (!condition)
        {
            AddError(_nextMessage ?? errorMessage);
        }
        
        _nextMessage = null;
    }

    public void ValidateAndAddError(bool condition, ValidationError error)
    {
        if (!condition)
        {
            if (_nextMessage != null)
            {
                AddError(_nextMessage);
            }
            else
            {
                AddError(error);
            }
        }
        
        _nextMessage = null;
    }
}

public abstract class FluentValidatorBase<T> : FluentValidatorBase
{
    public T Value { get; }
    public string PropertyName { get; }

    protected FluentValidatorBase(T value, string propertyName)
    {
        Value = value;
        PropertyName = propertyName;
    }
}
