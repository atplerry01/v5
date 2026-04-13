namespace Whycespace.Engines.T0U.WhyceChain.Command;

public sealed record AnchorEventsCommand(
    Guid CorrelationId,
    IReadOnlyList<object> Events,
    string DecisionHash,
    string PreviousBlockHash,
    long Sequence,
    long LastKnownSequence);
