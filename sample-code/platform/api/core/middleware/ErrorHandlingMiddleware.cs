using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// E19.17.6 — Error Normalization Middleware.
/// Catches all exceptions and returns a standardized error response.
///
/// GUARANTEES:
/// - No stack traces exposed to callers
/// - No internal exception details leaked
/// - Standardized error format: { status, error, message, traceId }
/// - All errors logged internally via ILogService
///
/// Runs as the OUTERMOST middleware in the pipeline.
/// </summary>
public sealed class ErrorHandlingMiddleware : IApiMiddleware
{
    private readonly ILogService _logService;

    public ErrorHandlingMiddleware(ILogService logService)
    {
        _logService = logService;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        try
        {
            var response = await next(request);

            // Normalize error responses to strip any internal details
            if (response.StatusCode >= 400 && response.Error is not null)
            {
                return NormalizeErrorResponse(response);
            }

            return response;
        }
        catch (PlatformLayerViolationException ex)
        {
            // Layer violation — critical, log and return 500
            _logService.LogError(
                request.TraceId ?? "unknown",
                request.Headers.GetValueOrDefault("X-Correlation-Id") ?? "unknown",
                "ErrorHandlingMiddleware",
                $"LAYER_VIOLATION: {ex.Message}");

            return CreateNormalizedError(500, "INTERNAL_ERROR", "A system error occurred", request.TraceId);
        }
        catch (Exception ex)
        {
            // Unhandled exception — log internally, return safe error
            _logService.LogError(
                request.TraceId ?? "unknown",
                request.Headers.GetValueOrDefault("X-Correlation-Id") ?? "unknown",
                "ErrorHandlingMiddleware",
                $"UNHANDLED: {ex.GetType().Name}: {ex.Message}");

            return CreateNormalizedError(500, "INTERNAL_ERROR", "An unexpected error occurred", request.TraceId);
        }
    }

    /// <summary>
    /// Normalizes error responses to use the standard error format.
    /// Strips any internal details that may have leaked through.
    /// </summary>
    private static ApiResponse NormalizeErrorResponse(ApiResponse response)
    {
        var (code, message) = response.StatusCode switch
        {
            400 => ("BAD_REQUEST", SanitizeErrorMessage(response.Error)),
            401 => ("UNAUTHORIZED", "Authentication required"),
            403 => ("ACCESS_DENIED", "Access not permitted"),
            404 => ("NOT_FOUND", "Resource not found"),
            429 => ("RATE_LIMITED", "Too many requests — retry later"),
            _ => ("INTERNAL_ERROR", "A system error occurred")
        };

        return CreateNormalizedError(response.StatusCode, code, message, response.TraceId);
    }

    private static ApiResponse CreateNormalizedError(int statusCode, string errorCode, string message, string? traceId)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Data = new NormalizedError
            {
                Status = "FAILED",
                Error = errorCode,
                Message = message
            },
            TraceId = traceId
        };
    }

    /// <summary>
    /// Strips internal details from error messages that are safe to return (400-level).
    /// Removes stack traces, type names, and internal identifiers.
    /// </summary>
    private static string SanitizeErrorMessage(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return "Invalid request";

        // Strip known internal prefixes that may leak through guard violations
        if (error.Contains("Platform guard violation:", StringComparison.OrdinalIgnoreCase))
            return "Request validation failed";

        // Keep user-facing classification/routing errors but strip details
        if (error.StartsWith("INVALID_INTENT:", StringComparison.OrdinalIgnoreCase))
            return "Invalid intent — check intent type and payload";

        if (error.StartsWith("ROUTING_FAILED:", StringComparison.OrdinalIgnoreCase))
            return "Unable to route request — check intent configuration";

        // For 400 errors from validation, keep the message as-is (schema errors are safe)
        return error;
    }
}

/// <summary>
/// Standardized error response body.
/// This is the ONLY error format returned to callers.
/// </summary>
public sealed record NormalizedError
{
    public required string Status { get; init; }
    public required string Error { get; init; }
    public required string Message { get; init; }
}
