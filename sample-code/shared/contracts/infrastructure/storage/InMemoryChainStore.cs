using System.Collections.Concurrent;
using System.Text.Json;

namespace Whycespace.Shared.Contracts.Infrastructure.Storage;

/// <summary>
/// In-memory implementation of IChainStore for development and testing.
/// Append-only — mirrors the PostgresChainStore contract exactly.
/// </summary>
public sealed class InMemoryChainStore : IChainStore
{
    private readonly List<ChainBlock> _blocks = [];
    private readonly object _lock = new();

    public Task<ChainBlock?> GetLatestBlockAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_blocks.Count > 0 ? _blocks[^1] : null);
        }
    }

    public Task AppendBlockAsync(ChainBlock block, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Enforce append-only: sequence must be contiguous
            var expectedSeq = _blocks.Count > 0 ? _blocks[^1].SequenceNumber + 1 : 1;
            if (block.SequenceNumber != expectedSeq)
                throw new InvalidOperationException(
                    $"Cannot append block with sequence {block.SequenceNumber}; expected {expectedSeq}.");

            // E2: Fork prevention — no two blocks may reference the same parent (DB-level defense)
            if (_blocks.Any(b => b.PreviousHash == block.PreviousHash))
                throw new InvalidOperationException(
                    $"FORK_PREVENTED: A block already references previous_hash [{block.PreviousHash}].");

            // Enforce chain linkage
            var expectedPrevHash = _blocks.Count > 0 ? _blocks[^1].Hash : ChainBlock.GenesisHash;
            if (block.PreviousHash != expectedPrevHash)
                throw new InvalidOperationException(
                    $"Block PreviousHash [{block.PreviousHash}] does not match last block hash [{expectedPrevHash}].");

            // E2: Idempotency — reject duplicate correlation IDs
            if (block.CorrelationId is not null
                && _blocks.Any(b => b.CorrelationId == block.CorrelationId))
                throw new InvalidOperationException(
                    $"DUPLICATE_CORRELATION: Block with correlation_id [{block.CorrelationId}] already exists.");

            _blocks.Add(block);
        }
        return Task.CompletedTask;
    }

    public Task<ChainBlock?> GetBlockByIdAsync(string blockId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_blocks.Find(b => b.BlockId == blockId));
        }
    }

    public Task<ChainBlock?> GetBlockByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_blocks.Find(b => b.Hash == hash));
        }
    }

    public Task<ChainBlock?> GetBlockBySequenceAsync(long sequenceNumber, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_blocks.Find(b => b.SequenceNumber == sequenceNumber));
        }
    }

    public Task<IReadOnlyList<ChainBlock>> GetBlocksAfterAsync(
        long sequenceNum, int limit = 100, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<ChainBlock> result = _blocks
                .Where(b => b.SequenceNumber > sequenceNum)
                .OrderBy(b => b.SequenceNumber)
                .Take(limit)
                .ToList()
                .AsReadOnly();
            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<ChainBlock>> GetBlocksByAggregateIdAsync(
        string aggregateId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<ChainBlock> result = _blocks
                .Where(b =>
                {
                    try
                    {
                        var doc = JsonDocument.Parse(b.Payload);
                        return doc.RootElement.TryGetProperty("aggregateId", out var prop)
                            && prop.GetString() == aggregateId;
                    }
                    catch { return false; }
                })
                .OrderBy(b => b.SequenceNumber)
                .ToList()
                .AsReadOnly();
            return Task.FromResult(result);
        }
    }

    public Task<long> GetChainLengthAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult((long)_blocks.Count);
        }
    }

    // Test helpers
    public IReadOnlyList<ChainBlock> GetAllBlocks()
    {
        lock (_lock) { return _blocks.ToList().AsReadOnly(); }
    }
}
