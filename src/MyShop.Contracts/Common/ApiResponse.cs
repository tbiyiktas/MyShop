namespace MyShop.Contracts.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public List<string> Errors { get; init; } = new();
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public T? Data { get; init; }

    public ApiResponse() { Errors = new List<string>(); }

    public static ApiResponse<T> SuccessResponse(T data)
        => new()
        {
            Success = true,
            Data = data
        };

    public static ApiResponse<T> ErrorResponse(params string[] errors)
        => new()
        {
            Success = false,
            Errors = errors?.ToList() ?? new List<string>()
        };

    public static ApiResponse<T> ErrorResponse(IEnumerable<string> errors)
        => new()
        {
            Success = false,
            Errors = errors?.ToList() ?? new List<string>()
        };
}
