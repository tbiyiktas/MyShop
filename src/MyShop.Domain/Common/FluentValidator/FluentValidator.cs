namespace MyShop.Domain.Common.FluentValidator;

public static class FluentValidator
{
    public static PrimitiveValidator<T> ForPrimitive<T>(T value, string name) =>
        new PrimitiveValidator<T>(value, name);

    public static ObjectValidator<T> ForObject<T>(T instance, string objectName) =>
        new ObjectValidator<T>(instance, objectName);

    public static FluentValidatorAggregate CreateAggregate() => 
        FluentValidatorAggregate.Create();

    public static PrimitiveValidator<bool> ForCondition(bool condition, string code) =>
       new PrimitiveValidator<bool>(condition, code);

    public static PrimitiveValidator<int> ForInt(int value, string name) => new(value, name);
    public static PrimitiveValidator<decimal> ForDecimal(decimal value, string name) => new(value, name);
    public static PrimitiveValidator<DateTime> ForDateTime(DateTime value, string name) => new(value, name);
    public static PrimitiveValidator<string> ForString(string value, string name) => new(value, name);
}
