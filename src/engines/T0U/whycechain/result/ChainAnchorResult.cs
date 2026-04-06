namespace Whyce.Engines.T0U.WhyceChain.Result;

public sealed record ChainAnchorResult(
    bool IsAnchored,
    string BlockHash,
    string EventHash,
    string PreviousBlockHash,
    long Sequence,
    string? FailureReason);
