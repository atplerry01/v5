namespace Whycespace.Shared.Contracts.Infrastructure.Chain;

public interface IChainAnchor
{
    // phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): the chain-store
    // adapter contract now accepts the request/host-shutdown CancellationToken
    // so the underlying ExecuteScalarAsync / ExecuteNonQueryAsync calls can
    // honor cancellation. The token is wait-only at the runtime layer (TC-2);
    // here it reaches the actual database round-trip.
    Task<ChainBlock> AnchorAsync(
        Guid correlationId,
        IReadOnlyList<object> events,
        string decisionHash,
        CancellationToken cancellationToken = default);
}

public sealed record ChainBlock(
    Guid BlockId,
    Guid CorrelationId,
    string EventHash,
    string DecisionHash,
    string PreviousBlockHash,
    DateTimeOffset Timestamp);
