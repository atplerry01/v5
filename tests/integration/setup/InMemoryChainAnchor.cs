using Whycespace.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// In-memory chain anchor sink. Determinism comes from injected IClock + IIdGenerator.
/// The cryptographic hash logic itself runs upstream in WhyceChainEngine via
/// ChainAnchorService — this sink only stores the resulting block.
///
/// Two test runs with identical inputs and identical IClock/IIdGenerator will
/// produce byte-equal blocks.
/// </summary>
public sealed class InMemoryChainAnchor : IChainAnchor
{
    private readonly object _lock = new();
    private readonly List<ChainBlock> _blocks = new();
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;
    private readonly StageRecorder? _recorder;

    public InMemoryChainAnchor(IClock clock, IIdGenerator idGenerator, StageRecorder? recorder = null)
    {
        _clock = clock;
        _idGenerator = idGenerator;
        _recorder = recorder;
    }

    public IReadOnlyList<ChainBlock> Blocks
    {
        get
        {
            lock (_lock) return _blocks.ToArray();
        }
    }

    public Task<ChainBlock> AnchorAsync(
        Guid correlationId,
        IReadOnlyList<object> events,
        string decisionHash,
        CancellationToken cancellationToken = default)
    {
        ChainBlock block;
        lock (_lock)
        {
            var sequence = _blocks.Count;
            var blockId = _idGenerator.Generate($"chain-block:{correlationId}:{sequence}");
            var previousHash = _blocks.Count == 0 ? "genesis" : _blocks[^1].EventHash;
            var eventHash = _idGenerator.Generate($"event-hash:{correlationId}:{sequence}").ToString();

            block = new ChainBlock(
                BlockId: blockId,
                CorrelationId: correlationId,
                EventHash: eventHash,
                DecisionHash: decisionHash,
                PreviousBlockHash: previousHash,
                Timestamp: _clock.UtcNow);

            _blocks.Add(block);
        }

        _recorder?.Record("Chain");
        return Task.FromResult(block);
    }
}
