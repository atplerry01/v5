namespace Whycespace.Shared.Contracts.Domain.Governance;

/// <summary>
/// Flattened, version-aware DTO for cross-context governance event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record GovernanceEventDTO : IGovernanceEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid SubjectId { get; init; }
    public required string SubjectType { get; init; }
    public required string DecisionOutcome { get; init; }
    public string? InitiatorId { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
    public int? VoteCount { get; init; }
    public int? QuorumThreshold { get; init; }
    public string? DelegationChain { get; init; }
}