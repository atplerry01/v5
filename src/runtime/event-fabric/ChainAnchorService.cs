using Whyce.Engines.T0U.WhyceChain.Command;
using Whyce.Engines.T0U.WhyceChain.Engine;
using Whyce.Engines.T0U.WhyceChain.Hashing;
using Whyce.Engines.T0U.WhyceChain.Result;
using Whyce.Shared.Contracts.Infrastructure.Chain;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Chain Anchor Service — anchors events to the immutable WhyceChain ledger.
/// MUST be invoked AFTER EventStore persistence, BEFORE Outbox.
/// Only invoked by the Event Fabric orchestrator.
///
/// Non-bypassable: No event persisted without WhyceChain anchoring.
/// Chain state (last block hash, sequence) is OWNED BY THIS RUNTIME SERVICE.
/// The engine is stateless — it receives state via the command.
/// </summary>
public sealed class ChainAnchorService
{
    private readonly WhyceChainEngine _chainEngine;
    private readonly IChainAnchor _chainAnchor;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string _lastBlockHash = ChainHasher.ComputeGenesisHash();
    private long _lastSequence = -1;

    public ChainAnchorService(WhyceChainEngine chainEngine, IChainAnchor chainAnchor)
    {
        _chainEngine = chainEngine;
        _chainAnchor = chainAnchor;
    }

    public async Task<ChainAnchorResult> AnchorAsync(
        Guid correlationId, IReadOnlyList<object> domainEvents, string policyHash)
    {
        await _lock.WaitAsync();
        try
        {
            var nextSequence = _lastSequence + 1;

            var anchorCommand = new AnchorEventsCommand(
                CorrelationId: correlationId,
                Events: domainEvents,
                DecisionHash: policyHash,
                PreviousBlockHash: _lastBlockHash,
                Sequence: nextSequence,
                LastKnownSequence: _lastSequence);

            var result = await _chainEngine.Anchor(anchorCommand);

            if (!result.IsAnchored)
            {
                throw new InvalidOperationException(
                    $"WhyceChain anchor failed: {result.FailureReason}. Chain integrity compromised.");
            }

            // Update runtime-owned chain state
            _lastBlockHash = result.BlockHash;
            _lastSequence = nextSequence;

            // Persist to external chain store
            await _chainAnchor.AnchorAsync(correlationId, domainEvents, policyHash);

            return result;
        }
        finally
        {
            _lock.Release();
        }
    }
}
