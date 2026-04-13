namespace Whycespace.Shared.Contracts.Common;

/// <summary>
/// Standard API response envelope. All endpoints return this shape.
/// Meta is guaranteed non-null by the factory methods.
/// </summary>
public sealed class ApiResponse<T>
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public required ResponseMeta Meta { get; init; }
    public ApiError? Error { get; init; }
}

/// <summary>
/// Factory methods for constructing standard API responses.
/// Every path populates Meta with correlationId + ISO 8601 timestamp.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, Guid correlationId, DateTimeOffset timestamp) => new()
    {
        Success = true,
        Data = data,
        Meta = BuildMeta(correlationId, timestamp)
    };

    public static ApiResponse<T> Ok<T>(T data, DateTimeOffset timestamp) => new()
    {
        Success = true,
        Data = data,
        Meta = BuildMeta(Guid.Empty, timestamp)
    };

    public static ApiResponse<object?> Fail(string code, string message, DateTimeOffset timestamp, Guid correlationId = default) => new()
    {
        Success = false,
        Data = null,
        Error = new ApiError(code, message),
        Meta = BuildMeta(correlationId, timestamp)
    };

    private static ResponseMeta BuildMeta(Guid correlationId, DateTimeOffset timestamp) => new()
    {
        CorrelationId = correlationId.ToString(),
        Timestamp = timestamp.ToString("o")
    };
}
