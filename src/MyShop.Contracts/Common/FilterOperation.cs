namespace MyShop.Contracts.Common;

public enum FilterOperation
{
    Equals,
    NotEquals,
    Contains,          // string
    StartsWith,        // string
    EndsWith,          // string
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}
