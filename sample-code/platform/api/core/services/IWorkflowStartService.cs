using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Starts workflows safely via WSS integration.
/// Flow: WhycePlus → DownstreamAdapter → ProcessHandler → WSS → Runtime.
/// Platform never calls engines or runtime directly — this service is the boundary.
///
/// MUST dispatch ONLY via WSS (DownstreamAdapter).
/// MUST NOT call runtime directly.
/// MUST NOT call engines.
/// MUST NOT execute business logic.
/// </summary>
public interface IWorkflowStartService
{
    Task<WorkflowStartResult> StartAsync(
        WorkflowStartRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to start a workflow through the WhycePlus gateway.
/// WorkflowId is deterministic — derived from correlationId + workflowKey + identityId.
/// Carries full execution context: routing, identity, policy preview metadata.
/// </summary>
public sealed record WorkflowStartRequest
{
    public required Guid WorkflowId { get; init; }
    public required string WorkflowKey { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }

    public required string Cluster { get; init; }
    public required string Authority { get; init; }
    public required string SubCluster { get; init; }
    public required string Domain { get; init; }
    public required string ExecutionTarget { get; init; }

    public required Guid IdentityId { get; init; }
    public required string WhyceId { get; init; }
    public required string CorrelationId { get; init; }
    public required string TraceId { get; init; }

    public string? Jurisdiction { get; init; }
    public TenantContext? Tenant { get; init; }
    public RegionContext? Region { get; init; }
    public IReadOnlyList<string>? Roles { get; init; }
    public IReadOnlyDictionary<string, string>? Attributes { get; init; }
    public decimal? TrustScore { get; init; }
    public IReadOnlyList<string>? Consents { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Result of a workflow start attempt.
/// </summary>
public sealed record WorkflowStartResult
{
    public required Guid WorkflowId { get; init; }
    public required string Status { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public bool IsAccepted => string.Equals(Status, "ACCEPTED", StringComparison.OrdinalIgnoreCase);

    public static WorkflowStartResult Accepted(Guid workflowId) => new()
    {
        WorkflowId = workflowId,
        Status = "ACCEPTED"
    };

    public static WorkflowStartResult Failed(Guid workflowId, string error, string? code = null) => new()
    {
        WorkflowId = workflowId,
        Status = "FAILED",
        ErrorMessage = error,
        ErrorCode = code
    };
}
