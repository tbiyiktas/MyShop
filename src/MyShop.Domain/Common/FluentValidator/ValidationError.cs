namespace MyShop.Domain.Common.FluentValidator;

public sealed class ValidationError
{
    public string Code { get; }
    public string Message { get; }
    public string? PropertyName { get; }
    public object? AttemptedValue { get; }

    public ValidationError(
        string code,
        string message,
        string? propertyName = null,
        object? attemptedValue = null)
    {
        Code = code;
        Message = message;
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }

    public override string ToString()
        => $"{Code} | {PropertyName}: {Message} (Value: {AttemptedValue})";
}
