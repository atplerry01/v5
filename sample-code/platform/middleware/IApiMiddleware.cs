namespace Whycespace.Platform.Middleware;

public interface IApiMiddleware
{
    Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next);
}

public sealed record ApiRequest
{
    public required string RequestId { get; init; }
    public required string Endpoint { get; init; }
    public required string Method { get; init; }
    public required object Body { get; init; }
    public string? BearerToken { get; init; }
    public string? WhyceId { get; init; }
    public string? TraceId { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}

public sealed record ApiResponse
{
    public required int StatusCode { get; init; }
    public object? Data { get; init; }
    public string? Error { get; init; }
    public string? TraceId { get; init; }

    public static ApiResponse Ok(object? data, string? traceId = null) =>
        new() { StatusCode = 200, Data = data, TraceId = traceId };

    public static ApiResponse Created(object? data, string? traceId = null) =>
        new() { StatusCode = 201, Data = data, TraceId = traceId };

    public static ApiResponse BadRequest(string error, string? traceId = null) =>
        new() { StatusCode = 400, Error = error, TraceId = traceId };

    public static ApiResponse Unauthorized(string? traceId = null) =>
        new() { StatusCode = 401, Error = "Unauthorized", TraceId = traceId };

    public static ApiResponse Forbidden(string error, string? traceId = null) =>
        new() { StatusCode = 403, Error = error, TraceId = traceId };

    public static ApiResponse NotFound(string error, string? traceId = null) =>
        new() { StatusCode = 404, Error = error, TraceId = traceId };

    public static ApiResponse ServerError(string error, string? traceId = null) =>
        new() { StatusCode = 500, Error = error, TraceId = traceId };
}
