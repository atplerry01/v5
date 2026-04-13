using Whycespace.Engines.T0U.WhyceChain.Hashing;

namespace Whycespace.Engines.T0U.WhyceChain.Integrity;

/// <summary>
/// Validates chain integrity by verifying hash linkage between blocks.
/// Any broken link = chain integrity compromised.
/// </summary>
public static class IntegrityValidator
{
    /// <summary>
    /// Validates that a block's hash correctly links to the previous block.
    /// </summary>
    public static (bool IsValid, string? Violation) ValidateBlockLink(
        string expectedPreviousHash,
        string actualPreviousHash)
    {
        if (!string.Equals(expectedPreviousHash, actualPreviousHash, StringComparison.Ordinal))
        {
            return (false,
                $"Chain link broken: expected previous hash '{expectedPreviousHash}', " +
                $"got '{actualPreviousHash}'. Chain integrity compromised.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates that a block hash can be recomputed from its components.
    /// </summary>
    public static (bool IsValid, string? Violation) ValidateBlockHash(
        string blockHash,
        string previousHash,
        string payloadHash,
        long sequence)
    {
        var recomputed = ChainHasher.ComputeBlockHash(previousHash, payloadHash, sequence);

        if (!string.Equals(blockHash, recomputed, StringComparison.Ordinal))
        {
            return (false,
                $"Block hash mismatch: stored '{blockHash}', " +
                $"recomputed '{recomputed}'. Block may be tampered.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates an entire chain of block entries.
    /// </summary>
    public static (bool IsValid, string? Violation) ValidateChain(
        IReadOnlyList<ChainEntry> entries)
    {
        if (entries.Count == 0)
            return (true, null);

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            // Validate block hash integrity
            var (hashValid, hashViolation) = ValidateBlockHash(
                entry.BlockHash, entry.PreviousBlockHash, entry.PayloadHash, entry.Sequence);

            if (!hashValid)
                return (false, $"Block {i}: {hashViolation}");

            // Validate chain linkage (skip genesis block)
            if (i > 0)
            {
                var (linkValid, linkViolation) = ValidateBlockLink(
                    entries[i - 1].BlockHash, entry.PreviousBlockHash);

                if (!linkValid)
                    return (false, $"Block {i}: {linkViolation}");
            }
        }

        return (true, null);
    }
}

/// <summary>
/// Represents a chain entry for integrity validation.
/// </summary>
public sealed record ChainEntry(
    string BlockHash,
    string PreviousBlockHash,
    string PayloadHash,
    long Sequence);
