namespace Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

public sealed record AnchorDescriptor(
    Guid CorrelationId,
    string BlockHash,
    string EventHash,
    string PreviousBlockHash,
    string DecisionHash,
    long Sequence);
