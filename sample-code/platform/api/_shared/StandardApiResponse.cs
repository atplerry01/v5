namespace Whycespace.Platform.Api.Shared;

public sealed record StandardApiResponse
{
    public required bool Success { get; init; }
    public object? Data { get; init; }
    public string? Error { get; init; }
    public string? TraceId { get; init; }
    public string? TrackingId { get; init; }

    public static StandardApiResponse Ok(object? data, string? traceId = null, string? trackingId = null) => new()
    {
        Success = true, Data = data, TraceId = traceId, TrackingId = trackingId
    };

    public static StandardApiResponse Created(object? data, string? traceId = null) => new()
    {
        Success = true, Data = data, TraceId = traceId
    };

    public static StandardApiResponse Fail(string error, string? traceId = null) => new()
    {
        Success = false, Error = error, TraceId = traceId
    };

    public static StandardApiResponse NotFound(string error, string? traceId = null) => new()
    {
        Success = false, Error = error, TraceId = traceId
    };

    public static StandardApiResponse FromPlatformResponse(Whycespace.Platform.Middleware.ApiResponse response)
    {
        return new StandardApiResponse
        {
            Success = response.StatusCode >= 200 && response.StatusCode < 300,
            Data = response.Data, Error = response.Error, TraceId = response.TraceId
        };
    }
}
