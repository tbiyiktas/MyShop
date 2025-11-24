using System.Text.RegularExpressions;

namespace MyShop.Domain.Common.FluentValidator;

public static class ValidationRules
{
    // Compiled Regexes for performance
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s\-().]{7,}$", RegexOptions.Compiled);
    private static readonly Regex CreditCardRegex = new(@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9]{2})[0-9]{12}|3[47][0-9]{13}|(6011|622(?:12|2[6-9]|3[0-5]|4[0-5]|6[0-5]|7[0-7]|8[0-9]|9[0-5])[0-9]{10})|(2131|1800|35\d{3})\d{11})$", RegexOptions.Compiled);

    public static bool IsEmail(string? value) => 
        !string.IsNullOrEmpty(value) && EmailRegex.IsMatch(value);

    public static bool IsPhoneNumber(string? value) => 
        !string.IsNullOrEmpty(value) && PhoneRegex.IsMatch(value);

    public static bool IsCreditCard(string? value) => 
        !string.IsNullOrEmpty(value) && CreditCardRegex.IsMatch(value);

    public static bool IsGuid(string? value) => 
        !string.IsNullOrEmpty(value) && Guid.TryParse(value, out _);

    public static bool IsDateTime(string? value) => 
        !string.IsNullOrEmpty(value) && DateTime.TryParse(value, out _);

    public static bool MatchesRegex(string? value, string pattern) => 
        value != null && Regex.IsMatch(value, pattern);
}
