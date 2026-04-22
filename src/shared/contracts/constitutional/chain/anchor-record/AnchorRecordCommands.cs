using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;

public sealed record RecordAnchorCommand(
    Guid AnchorRecordId,
    Guid CorrelationId,
    string BlockHash,
    string EventHash,
    string PreviousBlockHash,
    string DecisionHash,
    long Sequence,
    DateTimeOffset AnchoredAt) : IHasAggregateId
{
    public Guid AggregateId => AnchorRecordId;
}

public sealed record SealAnchorCommand(
    Guid AnchorRecordId,
    DateTimeOffset SealedAt) : IHasAggregateId
{
    public Guid AggregateId => AnchorRecordId;
}
