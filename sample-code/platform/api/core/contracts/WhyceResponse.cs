namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Standard response envelope for all WhycePlus platform responses.
/// Immutable. Returned by the intent gateway after downstream dispatch.
/// Includes optional PolicyPreview for advisory feedback to the caller.
/// </summary>
public sealed record WhyceResponse
{
    public required string Status { get; init; }
    public Guid? WorkflowId { get; init; }
    public object? Result { get; init; }
    public required string CorrelationId { get; init; }
    public string? TraceId { get; init; }
    public PolicyPreview? PolicyPreview { get; init; }

    public static WhyceResponse Accepted(
        Guid workflowId,
        string correlationId,
        string? traceId = null,
        PolicyPreview? policyPreview = null) => new()
    {
        Status = "ACCEPTED",
        WorkflowId = workflowId,
        CorrelationId = correlationId,
        TraceId = traceId,
        PolicyPreview = policyPreview
    };

    public static WhyceResponse Failed(
        string reason,
        string correlationId,
        string? traceId = null,
        PolicyPreview? policyPreview = null) => new()
    {
        Status = "FAILED",
        Result = new { Error = reason },
        CorrelationId = correlationId,
        TraceId = traceId,
        PolicyPreview = policyPreview
    };

    public static WhyceResponse Denied(
        string reason,
        string correlationId,
        string? traceId = null,
        PolicyPreview? policyPreview = null) => new()
    {
        Status = "DENIED",
        Result = new { Reason = reason },
        CorrelationId = correlationId,
        TraceId = traceId,
        PolicyPreview = policyPreview
    };

    public static WhyceResponse Ok(
        object data,
        string correlationId,
        string? traceId = null) => new()
    {
        Status = "OK",
        Result = data,
        CorrelationId = correlationId,
        TraceId = traceId
    };

    public static WhyceResponse NotFound(
        string resource,
        string correlationId,
        string? traceId = null) => new()
    {
        Status = "NOT_FOUND",
        Result = new { Resource = resource },
        CorrelationId = correlationId,
        TraceId = traceId
    };
}
