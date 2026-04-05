namespace Whyce.Shared.Contracts.Infrastructure.Chain;

public interface IChainAnchor
{
    Task<ChainBlock> AnchorAsync(Guid correlationId, IReadOnlyList<object> events, string decisionHash);
}

public sealed record ChainBlock(
    Guid BlockId,
    Guid CorrelationId,
    string EventHash,
    string DecisionHash,
    string PreviousBlockHash,
    DateTimeOffset Timestamp);
