namespace Whycespace.Platform.Api.Core.Contracts.Governance;

/// <summary>
/// Read-only governance decision projection view.
/// Exposes decision status and lifecycle — no internal vote data, no raw policy rules.
/// Sourced from CQRS projections — no domain access, no event replay.
/// </summary>
public sealed record GovernanceDecisionView
{
    public required Guid DecisionId { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public required string Cluster { get; init; }
    public required string Authority { get; init; }
    public Guid? ProposedBy { get; init; }
    public string? Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
}
