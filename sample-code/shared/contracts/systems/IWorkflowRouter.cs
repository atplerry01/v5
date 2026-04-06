using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Shared.Contracts.Systems;

/// <summary>
/// WSS workflow router contract.
/// Downstream calls WSS via this interface.
/// WSS resolves workflow definition, then dispatches intent to runtime boundary.
/// </summary>
public interface IWorkflowRouter
{
    Task<IntentResult> RouteAsync(WorkflowDispatchRequest request, CancellationToken cancellationToken = default);
}

public sealed record WorkflowDispatchRequest
{
    public required string WorkflowId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public required string Cluster { get; init; }
    public required string Subcluster { get; init; }
    public required string Domain { get; init; }
    public required string Context { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? AggregateId { get; init; }
    public string? WhyceId { get; init; }
    public string? PolicyId { get; init; }
    public GovernanceMetadata? GovernanceMetadata { get; init; }
}

/// <summary>
/// Governance traceability metadata attached to workflow dispatch requests.
/// Tracks actor identity and governance action for audit trail.
/// </summary>
public sealed record GovernanceMetadata
{
    public GovernanceAction Action { get; init; }
    public string? ActorId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string? ProposedBy { get; init; }
    public string? ApprovedBy { get; init; }
    public string? ActivatedBy { get; init; }
}

public enum GovernanceAction
{
    Unknown,
    Propose,
    Approve,
    Activate
}
