using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Engines.T3I.Observability.Chain;

/// <summary>
/// T3I engine: analyzes chain integrity from pre-fetched read model data.
/// Stateless, deterministic. No persistence, no repository dependencies.
/// </summary>
public sealed class ChainHealthEngine : IEngine<EvaluateChainHealthCommand>
{
    public Task<EngineResult> ExecuteAsync(
        EvaluateChainHealthCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();

        if (command.Blocks.Count == 0)
        {
            return Task.FromResult(EngineResult.Ok(new ChainHealthReport(
                IsHealthy: true,
                BlockHeight: 0,
                CurrentHead: ChainBlock.GenesisHash,
                Issues: ["Chain is empty — no blocks to validate."])));
        }

        var latestBlock = command.Blocks[^1];
        ChainBlock? previous = null;

        foreach (var block in command.Blocks)
        {
            var expectedHash = ChainBlock.ComputeHash(block.PreviousHash, block.PayloadHash, block.SequenceNumber);
            if (block.Hash != expectedHash)
            {
                issues.Add($"HASH_MISMATCH at seq {block.SequenceNumber}: stored [{block.Hash[..8]}...] != computed [{expectedHash[..8]}...]");
            }

            if (previous is not null && block.PreviousHash != previous.Hash)
            {
                issues.Add($"CONTINUITY_BREAK at seq {block.SequenceNumber}: previous_hash [{block.PreviousHash[..8]}...] != block[{previous.SequenceNumber}].hash [{previous.Hash[..8]}...]");
            }

            if (previous is not null && block.SequenceNumber != previous.SequenceNumber + 1)
            {
                issues.Add($"SEQUENCE_GAP: expected {previous.SequenceNumber + 1}, got {block.SequenceNumber}");
            }

            previous = block;
        }

        var report = new ChainHealthReport(
            IsHealthy: issues.Count == 0,
            BlockHeight: (int)latestBlock.SequenceNumber,
            CurrentHead: latestBlock.Hash,
            Issues: issues);

        return Task.FromResult(EngineResult.Ok(report));
    }
}

public sealed record EvaluateChainHealthCommand(
    IReadOnlyList<ChainBlock> Blocks);

public sealed record ChainHealthReport(
    bool IsHealthy,
    int BlockHeight,
    string CurrentHead,
    List<string> Issues);
