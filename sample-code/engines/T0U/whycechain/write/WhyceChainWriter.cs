using System.Text.Json;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhyceChain.Write;

/// <summary>
/// WhyceChain block writer — constitutional engine (T0U).
/// Append-only, deterministic, no update, no delete.
///
/// Hash = SHA256(PreviousHash + PayloadHash + SequenceNumber).
/// Timestamp is metadata only — NOT part of hash.
/// BlockId = first 32 hex chars of Hash (deterministic).
/// Replay MUST reconstruct an identical chain.
/// </summary>
public sealed class WhyceChainWriter : ChainEngineBase, IChainWriter
{
    private readonly IChainStore _chainStore;
    private readonly IClock _clock;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public WhyceChainWriter(IChainStore chainStore, IClock clock)
    {
        _chainStore = chainStore ?? throw new ArgumentNullException(nameof(chainStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Append a new block to the chain with the given payload.
    /// This method is SYNCHRONOUS with the event commit — failure here MUST fail the command.
    /// Thread-safe: only one block can be written at a time to maintain chain continuity.
    /// </summary>
    public async Task<ChainWriteResult> WriteAsync(
        ChainPayload payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            // Step 1: Compute SHA256 of serialized payload
            var payloadJson = JsonSerializer.Serialize(payload, ChainSerializerOptions.Default);
            var payloadHash = Whycespace.Shared.Utilities.HashUtility.ComputeSha256(payloadJson);

            // Step 2: Fetch last block to get previous hash and sequence
            var lastBlock = await _chainStore.GetLatestBlockAsync(cancellationToken);
            var previousHash = lastBlock?.Hash ?? ChainBlock.GenesisHash;
            var sequenceNumber = (lastBlock?.SequenceNumber ?? 0) + 1;

            // Step 3: Compute block hash — deterministic and reproducible
            // Timestamp is metadata only, deliberately excluded from hash
            var timestamp = _clock.UtcNowOffset;
            var hash = ChainBlock.ComputeHash(previousHash, payloadHash, sequenceNumber);

            // Step 4: Derive deterministic BlockId from hash
            var blockId = ChainBlock.DeriveBlockId(hash);

            // Step 5: Create and persist block (append-only)
            var block = new ChainBlock
            {
                BlockId = blockId,
                SequenceNumber = sequenceNumber,
                PreviousHash = previousHash,
                Hash = hash,
                PayloadHash = payloadHash,
                Payload = payloadJson,
                Timestamp = timestamp,
                CorrelationId = payload.CorrelationId
            };

            await _chainStore.AppendBlockAsync(block, cancellationToken);

            return new ChainWriteResult(
                blockId,
                hash,
                payloadHash,
                previousHash,
                sequenceNumber,
                timestamp);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}

internal static class ChainSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
