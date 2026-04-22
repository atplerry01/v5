namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;

public sealed record ConsistencyCheckReadModel
{
    public Guid CheckId { get; init; }
    public string ScopeTarget { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; init; }
    public bool? HasDiscrepancies { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
