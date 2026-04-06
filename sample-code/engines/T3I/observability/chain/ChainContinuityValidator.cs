using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Engines.T3I.Observability.Chain;

/// <summary>
/// T3I service: validates chain continuity by walking block[n].previous_hash == block[n-1].hash.
/// Stateless, deterministic. Accepts pre-fetched chain blocks as read model data.
/// </summary>
public sealed class ChainContinuityValidator
{
    /// <summary>
    /// Validates chain continuity across the provided block sequence.
    /// Blocks must be ordered by SequenceNumber ascending.
    /// </summary>
    public ContinuityReport Validate(IReadOnlyList<ChainBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        if (blocks.Count == 0)
            return new ContinuityReport(true, 0, []);

        var breaks = new List<ContinuityBreak>();
        ChainBlock? previous = null;

        foreach (var block in blocks)
        {
            if (previous is not null && block.PreviousHash != previous.Hash)
            {
                breaks.Add(new ContinuityBreak(
                    block.SequenceNumber,
                    block.BlockId,
                    ExpectedPreviousHash: previous.Hash,
                    ActualPreviousHash: block.PreviousHash));
            }

            var recomputed = ChainBlock.ComputeHash(block.PreviousHash, block.PayloadHash, block.SequenceNumber);
            if (block.Hash != recomputed)
            {
                breaks.Add(new ContinuityBreak(
                    block.SequenceNumber,
                    block.BlockId,
                    ExpectedPreviousHash: recomputed,
                    ActualPreviousHash: block.Hash,
                    IsHashCorruption: true));
            }

            if (previous is not null && block.SequenceNumber != previous.SequenceNumber + 1)
            {
                breaks.Add(new ContinuityBreak(
                    block.SequenceNumber,
                    block.BlockId,
                    ExpectedPreviousHash: $"seq:{previous.SequenceNumber + 1}",
                    ActualPreviousHash: $"seq:{block.SequenceNumber}",
                    IsSequenceGap: true));
            }

            previous = block;
        }

        return new ContinuityReport(
            IsValid: breaks.Count == 0,
            BlocksValidated: blocks.Count,
            Breaks: breaks);
    }
}

public sealed record ContinuityReport(
    bool IsValid,
    long BlocksValidated,
    List<ContinuityBreak> Breaks);

public sealed record ContinuityBreak(
    long SequenceNumber,
    string BlockId,
    string ExpectedPreviousHash,
    string ActualPreviousHash,
    bool IsHashCorruption = false,
    bool IsSequenceGap = false);
