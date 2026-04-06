using Whycespace.Platform.Middleware;
using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// E19.17.8 — Audit Logging Middleware.
/// Emits audit events for ALL platform requests (both reads and actions).
///
/// AUDIT SCOPE:
/// - All GET queries (read audit trail)
/// - All POST actions (action audit trail)
/// - All denied/failed requests (security audit trail)
///
/// GUARANTEES:
/// - Non-blocking — audit failures do NOT block request processing
/// - Emits to observability pipeline (Runtime → Observability)
/// - Includes identity, endpoint, method, status, correlation, and trace context
/// </summary>
public sealed class AuditLoggingMiddleware : IApiMiddleware
{
    private readonly ILogService _logService;
    private readonly IMetricsCollector _metrics;

    public AuditLoggingMiddleware(ILogService logService, IMetricsCollector metrics)
    {
        _logService = logService;
        _metrics = metrics;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;
        var traceId = request.TraceId ?? request.RequestId;
        var identityId = request.WhyceId ?? "anonymous";

        // Emit pre-execution audit (request received)
        EmitRequestAudit(traceId, correlationId, identityId, request);

        var response = await next(request);

        // Emit post-execution audit (response generated)
        EmitResponseAudit(traceId, correlationId, identityId, request, response);

        // Record audit metrics
        RecordAuditMetrics(request, response, identityId);

        return response;
    }

    private void EmitRequestAudit(string traceId, string correlationId, string identityId, ApiRequest request)
    {
        try
        {
            var auditType = IsReadRequest(request.Method) ? "QUERY" : "ACTION";
            _logService.LogCommand(
                traceId,
                correlationId,
                $"platform.audit.{auditType.ToLowerInvariant()}.request",
                $"RECEIVED: {request.Method} {request.Endpoint} by {identityId}");
        }
        catch
        {
            // Non-blocking — audit failure must not break request flow
        }
    }

    private void EmitResponseAudit(string traceId, string correlationId, string identityId,
        ApiRequest request, ApiResponse response)
    {
        try
        {
            var auditType = IsReadRequest(request.Method) ? "QUERY" : "ACTION";
            var status = response.StatusCode < 400 ? "SUCCESS" : "FAILED";

            _logService.LogCommand(
                traceId,
                correlationId,
                $"platform.audit.{auditType.ToLowerInvariant()}.response",
                $"{status}: {request.Method} {request.Endpoint} by {identityId} → {response.StatusCode}");

            // Security audit: log denied/unauthorized separately
            if (response.StatusCode is 401 or 403)
            {
                _logService.LogDecision(
                    traceId,
                    correlationId,
                    $"platform.security.{(response.StatusCode == 401 ? "unauthorized" : "forbidden")}",
                    allowed: false,
                    $"{request.Method} {request.Endpoint} by {identityId}");
            }
        }
        catch
        {
            // Non-blocking
        }
    }

    private void RecordAuditMetrics(ApiRequest request, ApiResponse response, string identityId)
    {
        try
        {
            var tags = new Dictionary<string, string>
            {
                ["endpoint"] = request.Endpoint,
                ["method"] = request.Method,
                ["status"] = response.StatusCode.ToString(),
                ["identity"] = identityId
            };

            _metrics.RecordCustomMetric("platform.audit.request", 1, tags);

            if (response.StatusCode >= 400)
            {
                _metrics.RecordCustomMetric("platform.audit.error", 1, tags);
            }
        }
        catch
        {
            // Non-blocking
        }
    }

    private static bool IsReadRequest(string method) =>
        string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method, "OPTIONS", StringComparison.OrdinalIgnoreCase);
}
