namespace Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;

public sealed record AnchorRecordReadModel
{
    public Guid AnchorRecordId { get; init; }
    public Guid CorrelationId { get; init; }
    public string BlockHash { get; init; } = string.Empty;
    public string EventHash { get; init; } = string.Empty;
    public string PreviousBlockHash { get; init; } = string.Empty;
    public string DecisionHash { get; init; } = string.Empty;
    public long Sequence { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset AnchoredAt { get; init; }
    public DateTimeOffset? SealedAt { get; init; }
}
