using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for appending blocks to WhyceChain.
/// Implementations live in T0U engine — runtime calls this via DI.
/// </summary>
public interface IChainWriter
{
    Task<ChainWriteResult> WriteAsync(ChainPayload payload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a chain block write operation.
/// </summary>
public sealed record ChainWriteResult(
    string BlockId,
    string Hash,
    string PayloadHash,
    string PreviousHash,
    long SequenceNumber,
    DateTimeOffset Timestamp);
