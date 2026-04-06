using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Contracts.Infrastructure.Storage;

/// <summary>
/// Infrastructure contract for WhyceChain immutable ledger persistence.
/// Implementations live in infrastructure/adapters — append-only, no updates, no deletes.
/// </summary>
public interface IChainStore
{
    Task<ChainBlock?> GetLatestBlockAsync(CancellationToken cancellationToken = default);
    Task AppendBlockAsync(ChainBlock block, CancellationToken cancellationToken = default);
    Task<ChainBlock?> GetBlockByIdAsync(string blockId, CancellationToken cancellationToken = default);
    Task<ChainBlock?> GetBlockByHashAsync(string hash, CancellationToken cancellationToken = default);
    Task<ChainBlock?> GetBlockBySequenceAsync(long sequenceNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainBlock>> GetBlocksAfterAsync(long sequenceNum, int limit = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainBlock>> GetBlocksByAggregateIdAsync(string aggregateId, CancellationToken cancellationToken = default);
    Task<long> GetChainLengthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Immutable chain block. BlockId is deterministic (derived from Hash).
/// Hash = SHA256(PreviousHash + PayloadHash + SequenceNumber).
/// Timestamp is metadata only — NOT part of hash computation.
/// Once written, blocks are NEVER updated or deleted.
/// </summary>
public sealed record ChainBlock
{
    /// <summary>
    /// Deterministic block identifier — derived from the block Hash.
    /// BlockId = first 32 hex chars of Hash.
    /// </summary>
    public required string BlockId { get; init; }

    public required long SequenceNumber { get; init; }
    public required string PreviousHash { get; init; }
    public required string Hash { get; init; }
    public required string PayloadHash { get; init; }
    public required string Payload { get; init; }

    /// <summary>
    /// Metadata only — NOT included in hash computation.
    /// Stored for observability. Does not affect determinism.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Correlation ID for idempotency enforcement (E2).
    /// Used to prevent duplicate block writes from retried commands.
    /// NOT part of hash computation. Nullable for pre-E2 blocks.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Hash = SHA256(PreviousHash + PayloadHash + SequenceNumber).
    /// Timestamp is deliberately excluded — same inputs MUST always produce the same hash
    /// regardless of when the block is written.
    /// </summary>
    public static string ComputeHash(string previousHash, string payloadHash, long sequenceNumber)
    {
        var input = $"{previousHash}{payloadHash}{sequenceNumber}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    public static string DeriveBlockId(string hash)
    {
        return hash[..32];
    }

    public const string GenesisHash = "0000000000000000000000000000000000000000000000000000000000000000";
}

/// <summary>
/// Health status result for WhyceChain infrastructure checks.
/// </summary>
public sealed record ChainHealthStatus(bool IsHealthy, string Message, long BlockCount);
