using System.Collections;
using System.Text.RegularExpressions;

namespace MyShop.Domain.Common.FluentValidator;

public class PrimitiveValidator<T> : FluentValidatorBase<T>
{
    public PrimitiveValidator(T value, string propertyName)
       : base(value, propertyName)
    { }

    public PrimitiveValidator<T> NotDefault()
    {
        ValidateAndAddError(!EqualityComparer<T>.Default.Equals(Value, default!), $"{PropertyName} cannot be the default value.");
        return this;
    }

    public PrimitiveValidator<T> NotNull()
    {
        ValidateAndAddError(Value != null, $"{PropertyName}: must not be null");
        return this;
    }

    public new void ValidateAndAddError(bool condition, string errorMessage)
    {
        base.ValidateAndAddError(condition, errorMessage);
    }

    public new void ValidateAndAddError(bool condition, ValidationError error)
    {
        base.ValidateAndAddError(condition, error);
    }

    public PrimitiveValidator<T> WithMessage(string errorMessage)
    {
        _nextMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage;
        return this;
    }

    public PrimitiveValidator<T> MustBeIn(IEnumerable<T> allowed, IEqualityComparer<T>? comparer = null)
    {
        var list = allowed ?? Array.Empty<T>();
        var ok = Value is not null && list.Contains(Value, comparer ?? EqualityComparer<T>.Default);

        var allowedPreview = "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";

        ValidateAndAddError(ok, $"{PropertyName}: must be one of {allowedPreview}");
        return this;
    }

    public PrimitiveValidator<T> MustBeIn(params T[] allowed)
        => MustBeIn((IEnumerable<T>)allowed);

    public PrimitiveValidator<T> MustNotBeIn(IEnumerable<T> disallowed, IEqualityComparer<T>? comparer = null)
    {
        var list = disallowed ?? Array.Empty<T>();
        var ok = Value is null || !list.Contains(Value, comparer ?? EqualityComparer<T>.Default);

        var disallowedPreview = "[" + string.Join(", ", list.Select(x => x?.ToString() ?? "null")) + "]";

        ValidateAndAddError(ok, $"{PropertyName}: must not be in {disallowedPreview}");
        return this;
    }

    public PrimitiveValidator<T> MustNotBeIn(params T[] disallowed)
        => MustNotBeIn((IEnumerable<T>)disallowed);

    public PrimitiveValidator<T> MustBeValidEnumName<TEnum>(
        bool ignoreCase = true,
        bool allowNumericString = false,
        bool allowCompositeFlags = true)
        where TEnum : struct, Enum
    {
        bool ok = false;

        if (Value is string s && !string.IsNullOrWhiteSpace(s))
        {
            if (!allowNumericString && long.TryParse(s, out _))
            {
                ok = false;
            }
            else if (Enum.TryParse<TEnum>(s, ignoreCase, out var parsed))
            {
                bool isFlags = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);
                ok = allowCompositeFlags && isFlags
                    ? IsValidFlagsCombination(parsed)
                    : Enum.IsDefined(typeof(TEnum), parsed);
            }
        }

        ValidateAndAddError(ok, $"{PropertyName}: must be a valid {typeof(TEnum).Name} name");
        return this;
    }

    public PrimitiveValidator<T> MustBeValidEnumValue<TEnum>(bool allowCompositeFlags = true)
        where TEnum : struct, Enum
    {
        bool ok = TryCoerceToEnum(Value, out TEnum parsed);

        if (ok)
        {
            bool isFlags = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);
            ok = allowCompositeFlags && isFlags
                ? IsValidFlagsCombination(parsed)
                : Enum.IsDefined(typeof(TEnum), parsed);
        }

        ValidateAndAddError(ok, $"{PropertyName}: must be a valid {typeof(TEnum).Name} value");
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
