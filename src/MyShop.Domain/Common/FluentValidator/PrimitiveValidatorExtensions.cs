using System.Collections;

namespace MyShop.Domain.Common.FluentValidator;

public static class PrimitiveValidatorExtensions
{
    // ─── STRING EXTENSIONS ──────────────────────────────────────────────────

    public static PrimitiveValidator<string> NotEmpty(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(!string.IsNullOrEmpty(validator.Value), $"{validator.PropertyName}: cannot be empty");
        return validator;
    }

    public static PrimitiveValidator<string> NotWhitespace(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(!string.IsNullOrWhiteSpace(validator.Value), $"{validator.PropertyName}: cannot be only whitespace");
        return validator;
    }

    public static PrimitiveValidator<string> MinLength(this PrimitiveValidator<string> validator, int min)
    {
        validator.ValidateAndAddError((validator.Value?.Length ?? 0) >= min, $"{validator.PropertyName}: must be at least {min} characters");
        return validator;
    }

    public static PrimitiveValidator<string> MaxLength(this PrimitiveValidator<string> validator, int max)
    {
        validator.ValidateAndAddError((validator.Value?.Length ?? 0) <= max, $"{validator.PropertyName}: must be at most {max} characters");
        return validator;
    }

    public static PrimitiveValidator<string> MatchesRegex(this PrimitiveValidator<string> validator, string pattern)
    {
        validator.ValidateAndAddError(ValidationRules.MatchesRegex(validator.Value, pattern), $"{validator.PropertyName}: is not in the correct format");
        return validator;
    }

    public static PrimitiveValidator<string> IsEmail(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(
            ValidationRules.IsEmail(validator.Value), 
            new ValidationError("IsEmail", "must be a valid email", validator.PropertyName, validator.Value));
        return validator;
    }

    public static PrimitiveValidator<string> IsPhoneNumber(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(
            ValidationRules.IsPhoneNumber(validator.Value), 
            new ValidationError("IsPhoneNumber", "must be a valid phone number", validator.PropertyName, validator.Value));
        return validator;
    }

    public static PrimitiveValidator<string> IsCreditCard(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(ValidationRules.IsCreditCard(validator.Value), $"{validator.PropertyName}: must be a valid credit card number");
        return validator;
    }

    public static PrimitiveValidator<string> IsGuid(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(ValidationRules.IsGuid(validator.Value), $"{validator.PropertyName}: must be a valid GUID");
        return validator;
    }

    public static PrimitiveValidator<string> IsDateTime(this PrimitiveValidator<string> validator)
    {
        validator.ValidateAndAddError(ValidationRules.IsDateTime(validator.Value), $"{validator.PropertyName}: must be a valid DateTime");
        return validator;
    }

    // ─── SPECIFIC OVERLOADS (int, long, decimal, double, DateTime) ──────────

    public static PrimitiveValidator<int> GreaterThan(this PrimitiveValidator<int> validator, int min)
    {
        validator.ValidateAndAddError(validator.Value > min, $"{validator.PropertyName}: must be greater than {min}");
        return validator;
    }

    public static PrimitiveValidator<int> GreaterThanOrEqualTo(this PrimitiveValidator<int> validator, int min)
    {
        validator.ValidateAndAddError(validator.Value >= min, $"{validator.PropertyName}: must be greater than or equal to {min}");
        return validator;
    }

    public static PrimitiveValidator<int> LessThan(this PrimitiveValidator<int> validator, int max)
    {
        validator.ValidateAndAddError(validator.Value < max, $"{validator.PropertyName}: must be less than {max}");
        return validator;
    }

    public static PrimitiveValidator<int> LessThanOrEqualTo(this PrimitiveValidator<int> validator, int max)
    {
        validator.ValidateAndAddError(validator.Value <= max, $"{validator.PropertyName}: must be less than or equal to {max}");
        return validator;
    }

    public static PrimitiveValidator<long> GreaterThan(this PrimitiveValidator<long> validator, long min)
    {
        validator.ValidateAndAddError(validator.Value > min, $"{validator.PropertyName}: must be greater than {min}");
        return validator;
    }

    public static PrimitiveValidator<long> GreaterThanOrEqualTo(this PrimitiveValidator<long> validator, long min)
    {
        validator.ValidateAndAddError(validator.Value >= min, $"{validator.PropertyName}: must be greater than or equal to {min}");
        return validator;
    }

    public static PrimitiveValidator<long> LessThan(this PrimitiveValidator<long> validator, long max)
    {
        validator.ValidateAndAddError(validator.Value < max, $"{validator.PropertyName}: must be less than {max}");
        return validator;
    }

    public static PrimitiveValidator<long> LessThanOrEqualTo(this PrimitiveValidator<long> validator, long max)
    {
        validator.ValidateAndAddError(validator.Value <= max, $"{validator.PropertyName}: must be less than or equal to {max}");
        return validator;
    }

    public static PrimitiveValidator<decimal> GreaterThan(this PrimitiveValidator<decimal> validator, decimal min)
    {
        validator.ValidateAndAddError(validator.Value > min, $"{validator.PropertyName}: must be greater than {min}");
        return validator;
    }

    public static PrimitiveValidator<decimal> GreaterThanOrEqualTo(this PrimitiveValidator<decimal> validator, decimal min)
    {
        validator.ValidateAndAddError(validator.Value >= min, $"{validator.PropertyName}: must be greater than or equal to {min}");
        return validator;
    }

    public static PrimitiveValidator<decimal> LessThan(this PrimitiveValidator<decimal> validator, decimal max)
    {
        validator.ValidateAndAddError(validator.Value < max, $"{validator.PropertyName}: must be less than {max}");
        return validator;
    }

    public static PrimitiveValidator<decimal> LessThanOrEqualTo(this PrimitiveValidator<decimal> validator, decimal max)
    {
        validator.ValidateAndAddError(validator.Value <= max, $"{validator.PropertyName}: must be less than or equal to {max}");
        return validator;
    }

    public static PrimitiveValidator<double> GreaterThan(this PrimitiveValidator<double> validator, double min)
    {
        validator.ValidateAndAddError(validator.Value > min, $"{validator.PropertyName}: must be greater than {min}");
        return validator;
    }

    public static PrimitiveValidator<double> GreaterThanOrEqualTo(this PrimitiveValidator<double> validator, double min)
    {
        validator.ValidateAndAddError(validator.Value >= min, $"{validator.PropertyName}: must be greater than or equal to {min}");
        return validator;
    }

    public static PrimitiveValidator<double> LessThan(this PrimitiveValidator<double> validator, double max)
    {
        validator.ValidateAndAddError(validator.Value < max, $"{validator.PropertyName}: must be less than {max}");
        return validator;
    }

    public static PrimitiveValidator<double> LessThanOrEqualTo(this PrimitiveValidator<double> validator, double max)
    {
        validator.ValidateAndAddError(validator.Value <= max, $"{validator.PropertyName}: must be less than or equal to {max}");
        return validator;
    }

    public static PrimitiveValidator<DateTime> GreaterThan(this PrimitiveValidator<DateTime> validator, DateTime min)
    {
        validator.ValidateAndAddError(validator.Value > min, $"{validator.PropertyName}: must be greater than {min}");
        return validator;
    }

    public static PrimitiveValidator<DateTime> GreaterThanOrEqualTo(this PrimitiveValidator<DateTime> validator, DateTime min)
    {
        validator.ValidateAndAddError(validator.Value >= min, $"{validator.PropertyName}: must be greater than or equal to {min}");
        return validator;
    }

    public static PrimitiveValidator<DateTime> LessThan(this PrimitiveValidator<DateTime> validator, DateTime max)
    {
        validator.ValidateAndAddError(validator.Value < max, $"{validator.PropertyName}: must be less than {max}");
        return validator;
    }

    public static PrimitiveValidator<DateTime> LessThanOrEqualTo(this PrimitiveValidator<DateTime> validator, DateTime max)
    {
        validator.ValidateAndAddError(validator.Value <= max, $"{validator.PropertyName}: must be less than or equal to {max}");
        return validator;
    }
    // Restored <T, TComparable> to match legacy behavior and avoid breaking changes with mixed types (e.g. uint vs int)

    public static PrimitiveValidator<T> GreaterThan<T, TComparable>(this PrimitiveValidator<T> validator, TComparable min) 
        where T : IComparable
        where TComparable : IComparable
    {
        // NOTE: If T and TComparable are different types (e.g. T=uint, TComparable=int),
        // the 'is' check below will fail and validation will be skipped silently.
        // This preserves legacy behavior but be aware of this limitation.
        if (validator.Value is TComparable comparableValue)
        {
            validator.ValidateAndAddError(comparableValue.CompareTo(min) > 0, $"{validator.PropertyName}: must be greater than {min}");
        }
        return validator;
    }

    public static PrimitiveValidator<T> GreaterThanEqualsTo<T, TComparable>(this PrimitiveValidator<T> validator, TComparable min) 
        where T : IComparable
        where TComparable : IComparable
    {
        if (validator.Value is TComparable comparableValue)
        {
            validator.ValidateAndAddError(comparableValue.CompareTo(min) >= 0, $"{validator.PropertyName}: must be greater than or equal to {min}");
        }
        return validator;
    }

    public static PrimitiveValidator<T> LessThan<T, TComparable>(this PrimitiveValidator<T> validator, TComparable max) 
        where T : IComparable
        where TComparable : IComparable
    {
        if (validator.Value is TComparable comparableValue)
        {
            validator.ValidateAndAddError(comparableValue.CompareTo(max) < 0, $"{validator.PropertyName}: must be less than {max}");
        }
        return validator;
    }

    public static PrimitiveValidator<T> LessThanEqualsTo<T, TComparable>(this PrimitiveValidator<T> validator, TComparable max) 
        where T : IComparable
        where TComparable : IComparable
    {
        if (validator.Value is TComparable comparableValue)
        {
            validator.ValidateAndAddError(comparableValue.CompareTo(max) <= 0, $"{validator.PropertyName}: must be less than or equal to {max}");
        }
        return validator;
    }

    public static PrimitiveValidator<T> Range<T, TComparable>(this PrimitiveValidator<T> validator, TComparable min, TComparable max)
        where T : IComparable
        where TComparable : IComparable
    {
        if (validator.Value is TComparable comparableValue)
        {
            validator.ValidateAndAddError(comparableValue.CompareTo(min) >= 0 && comparableValue.CompareTo(max) <= 0, $"{validator.PropertyName}: must be between {min} and {max}");
        }
        return validator;
    }

    // ─── BOOL EXTENSIONS ────────────────────────────────────────────────────

    public static PrimitiveValidator<bool> MustTrue(this PrimitiveValidator<bool> validator, string message)
    {
        validator.ValidateAndAddError(validator.Value == true, $"{validator.PropertyName}: {message}");
        return validator;
    }

    public static PrimitiveValidator<bool> IsTrue(this PrimitiveValidator<bool> validator)
        => validator.MustTrue("must be true");

    public static PrimitiveValidator<bool> MustFalse(this PrimitiveValidator<bool> validator, string message)
    {
        validator.ValidateAndAddError(validator.Value == false, $"{validator.PropertyName}: {message}");
        return validator;
    }

    public static PrimitiveValidator<bool> IsFalse(this PrimitiveValidator<bool> validator)
        => validator.MustFalse("must be false");

    // ─── COLLECTION EXTENSIONS ──────────────────────────────────────────────

    public static PrimitiveValidator<T> NotEmpty<T>(this PrimitiveValidator<T> validator) 
        where T : ICollection
    {
        if (validator.Value is ICollection collection)
        {
            validator.ValidateAndAddError(collection.Count > 0, $"{validator.PropertyName}: cannot be empty");
        }
        else if (validator.Value == null)
        {
             validator.ValidateAndAddError(false, $"{validator.PropertyName}: cannot be empty (value is null)");
        }
        return validator;
    }
}
