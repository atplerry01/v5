namespace Whycespace.Platform.Api.Core.Contracts.Governance;

/// <summary>
/// Read-only governance event in the decision timeline.
/// Tracks lifecycle transitions: PROPOSED → APPROVED → EXECUTED / REJECTED.
/// No internal vote details, no sensitive payloads.
/// </summary>
public sealed record GovernanceEventView
{
    public required string EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public Guid? ActorId { get; init; }
    public string? Description { get; init; }
}
