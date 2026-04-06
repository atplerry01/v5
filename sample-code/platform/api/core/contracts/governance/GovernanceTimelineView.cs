namespace Whycespace.Platform.Api.Core.Contracts.Governance;

/// <summary>
/// Read-only governance decision timeline projection view.
/// Chronological list of lifecycle events for a governance decision.
/// Sourced from CQRS projections — no event store access, no replay.
/// </summary>
public sealed record GovernanceTimelineView
{
    public required Guid DecisionId { get; init; }
    public required string Type { get; init; }
    public required string CurrentStatus { get; init; }
    public required IReadOnlyList<GovernanceEventView> Events { get; init; }
}
