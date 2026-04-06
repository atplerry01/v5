using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Engines.T3I.Observability.Chain;

/// <summary>
/// T3I engine: analyzes chain for corruption and produces replay manifests.
/// Stateless — accepts pre-fetched read model data. No repository dependencies.
/// Does NOT write to chain — produces reports for runtime to act on.
/// </summary>
public sealed class ChainRecoveryEngine : IEngine<ChainRecoveryCommand>
{
    private readonly ChainContinuityValidator _validator;

    public ChainRecoveryEngine(ChainContinuityValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public Task<EngineResult> ExecuteAsync(
        ChainRecoveryCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(command.Mode switch
        {
            RecoveryMode.DetectCorruption => DetectCorruption(command.Blocks),
            RecoveryMode.Replay => Replay(command.Blocks),
            RecoveryMode.ValidateAndReport => ValidateAndReport(command.Blocks),
            _ => EngineResult.Fail($"Unknown recovery mode: {command.Mode}", "UNKNOWN_RECOVERY_MODE")
        });
    }

    private EngineResult DetectCorruption(IReadOnlyList<ChainBlock> blocks)
    {
        var continuity = _validator.Validate(blocks);

        var allSegments = continuity.Breaks.Select(b => new CorruptedSegment(
            b.SequenceNumber, b.BlockId,
            b.IsHashCorruption ? "Hash corruption detected" :
            b.IsSequenceGap ? "Sequence gap detected" : "Linkage break detected")).ToList();

        return EngineResult.Ok(new ChainRecoveryReport(
            Mode: RecoveryMode.DetectCorruption,
            BlocksProcessed: continuity.BlocksValidated,
            CorruptedSegments: allSegments,
            IsClean: allSegments.Count == 0));
    }

    private static EngineResult Replay(IReadOnlyList<ChainBlock> blocks)
    {
        var replayed = blocks
            .Select(b => new ReplayedBlock(b.BlockId, b.SequenceNumber, b.Hash, b.Payload))
            .ToList();

        return EngineResult.Ok(new ChainRecoveryReport(
            Mode: RecoveryMode.Replay,
            BlocksProcessed: replayed.Count,
            CorruptedSegments: [],
            IsClean: true,
            ReplayedBlocks: replayed));
    }

    private EngineResult ValidateAndReport(IReadOnlyList<ChainBlock> blocks)
    {
        var continuity = _validator.Validate(blocks);

        return EngineResult.Ok(new ChainRecoveryReport(
            Mode: RecoveryMode.ValidateAndReport,
            BlocksProcessed: continuity.BlocksValidated,
            CorruptedSegments: continuity.Breaks
                .Select(b => new CorruptedSegment(b.SequenceNumber, b.BlockId,
                    b.IsHashCorruption ? "Hash corruption" :
                    b.IsSequenceGap ? "Sequence gap" : "Linkage break"))
                .ToList(),
            IsClean: continuity.IsValid));
    }
}

public sealed record ChainRecoveryCommand(
    IReadOnlyList<ChainBlock> Blocks,
    RecoveryMode Mode = RecoveryMode.ValidateAndReport);

public enum RecoveryMode
{
    DetectCorruption,
    Replay,
    ValidateAndReport
}

public sealed record ChainRecoveryReport(
    RecoveryMode Mode,
    long BlocksProcessed,
    List<CorruptedSegment> CorruptedSegments,
    bool IsClean,
    List<ReplayedBlock>? ReplayedBlocks = null);

public sealed record CorruptedSegment(
    long SequenceNumber,
    string BlockId,
    string Reason);

public sealed record ReplayedBlock(
    string BlockId,
    long SequenceNumber,
    string Hash,
    string Payload);
