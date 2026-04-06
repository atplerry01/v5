using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Jobs.Chain;

/// <summary>
/// Periodic runtime job that runs chain health checks and emits observability events.
/// Runs every N seconds (configurable). Does NOT modify chain data.
///
/// Flow: Runtime Job → IChainStore (read-only) → Health Report → Observability Events → Alert Dispatcher
///
/// Note: The full T3I ChainHealthEngine is invoked via engine dispatch in production.
/// This job performs lightweight inline validation for the runtime layer.
/// </summary>
public sealed class ChainHealthMonitorJob
{
    private readonly IChainStore _chainStore;
    private readonly IClock _clock;
    private readonly IEventPublisher? _eventPublisher;
    private readonly ChainAlertDispatcher? _alertDispatcher;
    private readonly int _validationDepth;

    public ChainHealthMonitorJob(
        IChainStore chainStore,
        IClock clock,
        IEventPublisher? eventPublisher = null,
        ChainAlertDispatcher? alertDispatcher = null,
        int validationDepth = 100)
    {
        _chainStore = chainStore ?? throw new ArgumentNullException(nameof(chainStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventPublisher = eventPublisher;
        _alertDispatcher = alertDispatcher;
        _validationDepth = validationDepth;
    }

    /// <summary>
    /// Execute one health check cycle. Called periodically by the runtime scheduler.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = $"chain-health-{DeterministicIdHelper.FromSeed($"chain-health:{_clock.UtcNowOffset:yyyyMMddHHmmss}"):N}";
        var issues = new List<string>();

        var chainLength = await _chainStore.GetChainLengthAsync(cancellationToken);
        var latestBlock = await _chainStore.GetLatestBlockAsync(cancellationToken);

        if (chainLength == 0 || latestBlock is null)
        {
            await EmitHealthCheckedAsync(true, 0, ChainBlock.GenesisHash, [], correlationId, cancellationToken);
            return;
        }

        // Validate recent blocks (configurable depth)
        var depth = Math.Min(_validationDepth, chainLength);
        var startSeq = Math.Max(1, latestBlock.SequenceNumber - depth + 1);
        var blocks = await _chainStore.GetBlocksAfterAsync(startSeq - 1, (int)depth, cancellationToken);

        ChainBlock? previous = null;
        if (startSeq > 1)
            previous = await _chainStore.GetBlockBySequenceAsync(startSeq - 1, cancellationToken);

        foreach (var block in blocks)
        {
            var expectedHash = ChainBlock.ComputeHash(block.PreviousHash, block.PayloadHash, block.SequenceNumber);
            if (block.Hash != expectedHash)
                issues.Add($"HASH_MISMATCH at seq {block.SequenceNumber}");

            if (previous is not null && block.PreviousHash != previous.Hash)
                issues.Add($"CONTINUITY_BREAK at seq {block.SequenceNumber}");

            if (previous is not null && block.SequenceNumber != previous.SequenceNumber + 1)
                issues.Add($"SEQUENCE_GAP at seq {block.SequenceNumber}");

            previous = block;
        }

        var isHealthy = issues.Count == 0;
        await EmitHealthCheckedAsync(isHealthy, (int)latestBlock.SequenceNumber, latestBlock.Hash, issues, correlationId, cancellationToken);

        if (!isHealthy && _alertDispatcher is not null)
        {
            foreach (var issue in issues)
            {
                await _alertDispatcher.DispatchAsync(
                    ChainAlertType.AnomalyDetected,
                    issue,
                    correlationId,
                    cancellationToken);
            }
        }
    }

    private async Task EmitHealthCheckedAsync(
        bool isHealthy, int blockHeight, string currentHead,
        List<string> issues, string correlationId, CancellationToken cancellationToken)
    {
        if (_eventPublisher is null) return;

        await _eventPublisher.PublishAsync(new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"chain-health:checked:{correlationId}"),
                AggregateId = Guid.Empty,
            AggregateType = "WhyceChain",
            EventType = "whyce.observability.chain.health.checked",
            CorrelationId = correlationId,
            Payload = new { IsHealthy = isHealthy, BlockHeight = blockHeight, CurrentHead = currentHead, Issues = issues },
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);

        if (!isHealthy)
        {
            await _eventPublisher.PublishAsync(new RuntimeEvent
            {
                EventId = DeterministicIdHelper.FromSeed($"chain-health:anomaly:{correlationId}"),
                AggregateId = Guid.Empty,
                AggregateType = "WhyceChain",
                EventType = "whyce.observability.chain.anomaly.detected",
                CorrelationId = correlationId,
                Payload = new { Issues = issues, BlockHeight = blockHeight, CurrentHead = currentHead, DetectedAt = _clock.UtcNowOffset },
                Timestamp = _clock.UtcNowOffset
            }, cancellationToken);
        }
    }
}
