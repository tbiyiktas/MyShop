using MyShop.Domain.Common.FluentValidator;

namespace MyShop.Domain.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool HasError => !IsSuccess;
    public List<string> Errors { get; protected set; } = new();
    public List<ValidationError> ValidationErrors { get; protected set; } = new();

    protected Result(bool isSuccess, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        if (errors != null)
        {
            Errors = errors;
        }
    }

    protected Result(bool isSuccess, List<ValidationError>? validationErrors)
    {
        IsSuccess = isSuccess;
        if (validationErrors != null)
        {
            ValidationErrors = validationErrors;
            Errors = validationErrors.Select(e => e.ToString()).ToList();
        }
    }

    public static Result Success() => new(true, (List<string>?)null);
    public static Result Failed(List<string> errors) => new(false, errors);
    public static Result Failed(params string[] errors) => new(false, errors.ToList());
    public static Result Failed(List<ValidationError> errors) => new(false, errors);
    public static Result Failed(params ValidationError[] errors) => new(false, errors.ToList());
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value, List<string>? errors = null) 
        : base(isSuccess, errors)
    {
        Value = value;
    }

    private Result(bool isSuccess, T? value, List<ValidationError>? validationErrors)
        : base(isSuccess, validationErrors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, (List<string>?)null);
    public new static Result<T> Failed(List<string> errors) => new(false, default, errors);
    public new static Result<T> Failed(params string[] errors) => new(false, default, errors.ToList());
    public new static Result<T> Failed(List<ValidationError> errors) => new(false, default, errors);
    public new static Result<T> Failed(params ValidationError[] errors) => new(false, default, errors.ToList());
}
