namespace Whycespace.Platform.Api.Core.Contracts.Audit;

/// <summary>
/// Read-only end-to-end execution trace.
/// Provides full traceability: Command → Workflow → Steps → Policy → Chain.
/// CorrelationId is the primary lookup key.
/// Sourced from pre-built trace projections — no event store queries, no reconstruction.
///
/// Does NOT expose: raw event payloads, internal commands, sensitive policy rules.
/// Exposes only: summarized trace views for audit and visibility.
/// </summary>
public sealed record TraceView
{
    public required string CorrelationId { get; init; }
    public Guid? WorkflowId { get; init; }
    public required string WorkflowKey { get; init; }
    public required string Status { get; init; }
    public required string Cluster { get; init; }
    public Guid? IdentityId { get; init; }
    public required IReadOnlyList<TraceStepView> Steps { get; init; }
    public PolicyTraceView? Policy { get; init; }
    public ChainTraceView? Chain { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
