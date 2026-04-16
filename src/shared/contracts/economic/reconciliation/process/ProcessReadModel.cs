namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Process;

public sealed record ProcessReadModel
{
    public Guid ProcessId { get; init; }
    public Guid LedgerReference { get; init; }
    public Guid ObservedReference { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset TriggeredAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
