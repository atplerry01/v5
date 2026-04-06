using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Engines.T0U.WhyceChain.Verification;

/// <summary>
/// WhyceChain verification engine — tamper detection and audit trail.
///
/// verifyBlock(blockId)       — re-computes hash, compares to stored
/// verifyChainIntegrity()     — walks entire chain, validates all links
/// getAuditTrail(aggregateId) — returns all blocks for a given aggregate
///
/// If computed hash != stored hash → CHAIN_INTEGRITY_VIOLATION
/// </summary>
public sealed class ChainVerificationEngine : ChainEngineBase
{
    private readonly IChainStore _chainStore;

    public ChainVerificationEngine(IChainStore chainStore)
    {
        _chainStore = chainStore ?? throw new ArgumentNullException(nameof(chainStore));
    }

    /// <summary>
    /// Verify a single block by re-computing its hash from constituents.
    /// Throws ChainIntegrityViolationException if hashes diverge.
    /// </summary>
    public async Task<BlockVerificationResult> VerifyBlockAsync(
        string blockId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blockId);

        var block = await _chainStore.GetBlockByIdAsync(blockId, cancellationToken)
            ?? throw new KeyNotFoundException($"Block '{blockId}' not found in chain.");

        var computedHash = ChainBlock.ComputeHash(
            block.PreviousHash,
            block.PayloadHash,
            block.SequenceNumber);

        if (computedHash != block.Hash)
        {
            throw new ChainIntegrityViolationException(
                block.BlockId,
                block.SequenceNumber,
                block.Hash,
                computedHash);
        }

        return new BlockVerificationResult(
            block.BlockId,
            block.SequenceNumber,
            IsValid: true,
            block.Hash);
    }

    /// <summary>
    /// Walk the entire chain from genesis, verifying:
    /// 1. Each block's hash matches re-computation
    /// 2. Each block's PreviousHash matches the prior block's Hash
    /// 3. SequenceNumbers are contiguous (no gaps)
    ///
    /// Throws ChainIntegrityViolationException on first tamper detected.
    /// </summary>
    public async Task<ChainIntegrityResult> VerifyChainIntegrityAsync(
        CancellationToken cancellationToken = default)
    {
        var chainLength = await _chainStore.GetChainLengthAsync(cancellationToken);
        if (chainLength == 0)
            return new ChainIntegrityResult(IsValid: true, BlocksVerified: 0, Violations: []);

        var violations = new List<string>();
        long verified = 0;
        string expectedPreviousHash = ChainBlock.GenesisHash;

        // Walk chain from sequence 1 in batches
        long currentSeq = 0;
        const int batchSize = 100;

        while (true)
        {
            var blocks = await _chainStore.GetBlocksAfterAsync(currentSeq, batchSize, cancellationToken);
            if (blocks.Count == 0)
                break;

            foreach (var block in blocks)
            {
                // Verify contiguous sequence
                if (block.SequenceNumber != currentSeq + 1)
                {
                    violations.Add(
                        $"Sequence gap: expected {currentSeq + 1}, found {block.SequenceNumber} at block {block.BlockId}.");
                }

                // Verify previous hash linkage
                if (block.PreviousHash != expectedPreviousHash)
                {
                    violations.Add(
                        $"Link broken at block {block.BlockId} (seq {block.SequenceNumber}): " +
                        $"PreviousHash [{block.PreviousHash}] != expected [{expectedPreviousHash}].");
                }

                // Verify block hash (timestamp excluded — deterministic)
                var computedHash = ChainBlock.ComputeHash(
                    block.PreviousHash,
                    block.PayloadHash,
                    block.SequenceNumber);

                if (computedHash != block.Hash)
                {
                    throw new ChainIntegrityViolationException(
                        block.BlockId,
                        block.SequenceNumber,
                        block.Hash,
                        computedHash);
                }

                expectedPreviousHash = block.Hash;
                currentSeq = block.SequenceNumber;
                verified++;
            }
        }

        return new ChainIntegrityResult(
            IsValid: violations.Count == 0,
            BlocksVerified: verified,
            Violations: violations);
    }

    /// <summary>
    /// Returns all chain blocks associated with a given aggregate, ordered by sequence.
    /// </summary>
    public async Task<IReadOnlyList<ChainBlock>> GetAuditTrailAsync(
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateId);
        return await _chainStore.GetBlocksByAggregateIdAsync(aggregateId, cancellationToken);
    }
}

public sealed record BlockVerificationResult(
    string BlockId,
    long SequenceNumber,
    bool IsValid,
    string Hash);

public sealed record ChainIntegrityResult(
    bool IsValid,
    long BlocksVerified,
    IReadOnlyList<string> Violations);
