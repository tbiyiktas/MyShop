using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace MyShop.Domain.Common.FluentValidator;

public class ObjectValidator<T> : FluentValidatorBase<T>
{
    private static readonly ConcurrentDictionary<Expression, Delegate> _compiledExpressionCache = new();
    public ObjectValidator(T instance, string objectName)
        : base(instance, objectName)
    {
    }

    private (string PropertyName, TProp? Value) GetPropertyInfoAndValue<TProp>(Expression<Func<T, TProp>> expression)
    {
        var name = GetPropertyName(expression);

        if (!_compiledExpressionCache.TryGetValue(expression, out var compiledDelegate))
        {
            compiledDelegate = expression.Compile();
            _compiledExpressionCache.TryAdd(expression, compiledDelegate);
        }

        TProp? value = default;
        if (Value != null)
        {
            var func = (Func<T, TProp>)compiledDelegate;
            value = func(Value);
        }

        return (name, value);
    }

    private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> expression)
    {
        var body = expression.Body;
        
        // Handle boxing (e.g. x => x.IntProp as object)
        if (body is UnaryExpression unary)
        {
            body = unary.Operand;
        }

        if (body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        throw new ArgumentException("Expression must be a member access expression.", nameof(expression));
    }

    public ObjectValidator<T> NotDefault<TProp>(Expression<Func<T, TProp>> expression)
    {
        var (name, propValueObj) = GetPropertyInfoAndValue(expression);
        var propValue = propValueObj is TProp v ? v : default!;

        ValidateAndAddError(!EqualityComparer<TProp>.Default.Equals(propValue, default!), $"{name} cannot be the default value.");
        return this;
    }

    public ObjectValidator<T> NotNull<TProp>(Expression<Func<T, TProp>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(propValue != null, $"{name}: must not be null");
        return this;
    }

    public ObjectValidator<T> NotEmpty(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(!string.IsNullOrEmpty(propValue), $"{name}: cannot be empty");
        }
        else
        {
            ValidateAndAddError(false, $"{name}: cannot be empty (value is null)");
        }

        return this;
    }

    public ObjectValidator<T> NotEmpty<TProp>(Expression<Func<T, TProp>> expression)
        where TProp : ICollection
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(propValue.Count > 0, $"{name}: cannot be empty");
        }
        else
        {
            ValidateAndAddError(false, $"{name}: cannot be empty (value is null)");
        }
        return this;
    }

    public ObjectValidator<T> NotWhitespace(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(!string.IsNullOrWhiteSpace(propValue), $"{name}: cannot be only whitespace");
        }

        return this;
    }

    public ObjectValidator<T> MinLength(Expression<Func<T, string>> expression, int min)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError((propValue?.Length ?? 0) >= min, $"{name}: must be at least {min} characters");
        }

        return this;
    }

    public ObjectValidator<T> MaxLength(Expression<Func<T, string>> expression, int max)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError((propValue?.Length ?? 0) <= max, $"{name}: must be at most {max} characters");
        }
        return this;
    }


    public ObjectValidator<T> MatchesRegex(Expression<Func<T, string>> expression, string pattern)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(ValidationRules.MatchesRegex(propValue, pattern), $"{name}: is not in the correct format");
        return this;
    }

    public ObjectValidator<T> GreaterThan<TProp>(Expression<Func<T, TProp>> expression, TProp min)
       where TProp : IComparable<TProp>
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(propValue.CompareTo(min) > 0, $"{name}: must be greater than {min}");
        }

        return this;
    }

    public ObjectValidator<T> GreaterThanEqualsTo<TProp>(Expression<Func<T, TProp>> expression, TProp min)
        where TProp : IComparable<TProp>
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(propValue.CompareTo(min) >= 0, $"{name}: must be greater than or equal to {min}");
        }

        return this;
    }

    public ObjectValidator<T> LessThan<TProp>(Expression<Func<T, TProp>> expression, TProp max)
        where TProp : IComparable<TProp>
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(propValue.CompareTo(max) < 0, $"{name}: must be less than {max}");
        }

        return this;
    }

    public ObjectValidator<T> LessThanEqualsTo<TProp>(Expression<Func<T, TProp>> expression, TProp max)
        where TProp : IComparable<TProp>
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(propValue.CompareTo(max) <= 0, $"{name}: must be less than or equal to {max}");
        }

        return this;
    }

    public ObjectValidator<T> Range<TProp>(Expression<Func<T, TProp>> expression, TProp min, TProp max)
    where TProp : IComparable<TProp>
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        if (propValue != null)
        {
            ValidateAndAddError(propValue.CompareTo(min) >= 0 && propValue.CompareTo(max) <= 0, $"{name}: must be between {min} and {max}");
        }

        return this;
    }

    public ObjectValidator<T> IsEmail(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(ValidationRules.IsEmail(propValue), $"{name}: must be a valid email");
        return this;
    }

    public ObjectValidator<T> IsGuid(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(ValidationRules.IsGuid(propValue), $"{name}: must be a valid GUID");
        return this;
    }

    public ObjectValidator<T> IsDateTime(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(ValidationRules.IsDateTime(propValue), $"{name}: must be a valid DateTime");
        return this;
    }

    public ObjectValidator<T> IsPhoneNumber(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(ValidationRules.IsPhoneNumber(propValue), $"{name}: must be a valid phone number");
        return this;
    }

    public ObjectValidator<T> IsCreditCard(Expression<Func<T, string>> expression)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        ValidateAndAddError(ValidationRules.IsCreditCard(propValue), $"{name}: must be a valid credit card number");
        return this;
    }

    public ObjectValidator<T> WithMessage(string errorMessage)
    {
        _nextMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage;
        return this;
    }

    public ObjectValidator<T> MustBeIn<TProp>(
        Expression<Func<T, TProp>> expression,
        IEnumerable<TProp> allowed,
        IEqualityComparer<TProp>? comparer = null)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        var list = allowed ?? Enumerable.Empty<TProp>();

        var ok = propValue is not null && list.Contains(propValue, comparer ?? EqualityComparer<TProp>.Default);

        var preview = "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";
        ValidateAndAddError(ok, $"{name}: must be one of {preview}");
        return this;
    }

    public ObjectValidator<T> MustBeIn<TProp>(
        Expression<Func<T, TProp>> expression,
        params TProp[] allowed)
        => MustBeIn(expression, (IEnumerable<TProp>)allowed);

    public ObjectValidator<T> MustNotBeIn<TProp>(
        Expression<Func<T, TProp>> expression,
        IEnumerable<TProp> disallowed,
        IEqualityComparer<TProp>? comparer = null)
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        var list = disallowed ?? Enumerable.Empty<TProp>();

        var ok = Value == null || !list.Contains(propValue!, comparer ?? EqualityComparer<TProp>.Default);

        var preview = "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";
        ValidateAndAddError(ok, $"{name}: must not be in {preview}");
        return this;
    }

    public ObjectValidator<T> MustNotBeIn<TProp>(
        Expression<Func<T, TProp>> expression,
        params TProp[] disallowed)
        => MustNotBeIn(expression, (IEnumerable<TProp>)disallowed);

    public ObjectValidator<T> MustBeValidEnumName<TEnum>(
        Expression<Func<T, string>> expression,
        bool ignoreCase = true,
        bool allowNumericString = false,
        bool allowCompositeFlags = true)
        where TEnum : struct, Enum
    {
        var (name, propValue) = GetPropertyInfoAndValue(expression);
        bool ok = false;

        if (!string.IsNullOrWhiteSpace(propValue))
        {
            if (!allowNumericString && long.TryParse(propValue, out _))
            {
                ok = false;
            }
            else if (Enum.TryParse<TEnum>(propValue, ignoreCase, out var parsed))
            {
                bool isFlags = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);
                ok = allowCompositeFlags && isFlags
                    ? IsValidFlagsCombination(parsed)
                    : Enum.IsDefined(typeof(TEnum), parsed);
            }
        }

        ValidateAndAddError(ok, $"{name}: must be a valid {typeof(TEnum).Name} name");
        return this;
    }

    public ObjectValidator<T> MustBeValidEnumValue<TEnum>(
        Expression<Func<T, object?>> expression,
        bool allowCompositeFlags = true)
        where TEnum : struct, Enum
    {
        var (name, propValueObj) = GetPropertyInfoAndValue(expression);
        bool ok = TryCoerceToEnum(propValueObj, out TEnum parsed);

        if (ok)
        {
            bool isFlags = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);
            ok = allowCompositeFlags && isFlags
                ? IsValidFlagsCombination(parsed)
                : Enum.IsDefined(typeof(TEnum), parsed);
        }

        ValidateAndAddError(ok, $"{name}: must be a valid {typeof(TEnum).Name} value");
        return this;
    }

    private static bool TryCoerceToEnum<TEnum>(object? value, out TEnum parsed) where TEnum : struct, Enum
    {
        parsed = default;
        if (value is null) return false;

        if (value is TEnum e)
        {
            parsed = e;
            return true;
        }

        if (value is string s && Enum.TryParse<TEnum>(s, true, out var e1))
        {
            parsed = e1;
            return true;
        }

        if (value is IConvertible)
        {
            try
            {
                var v = Convert.ToInt64(value);
                parsed = (TEnum)Enum.ToObject(typeof(TEnum), v);
                return true;
            }
            catch { /* ignore */ }
        }

        return false;
    }

    private static bool IsValidFlagsCombination<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        long v = Convert.ToInt64(value);
        long all = 0;
        foreach (var item in Enum.GetValues(typeof(TEnum)))
            all |= Convert.ToInt64(item);

        bool zeroOk = Enum.IsDefined(typeof(TEnum), 0);
        return (v & ~all) == 0 && (v != 0 || zeroOk);
    }
}
