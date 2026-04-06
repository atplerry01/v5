using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Shared.Contracts.Chain;

namespace Whycespace.Runtime.WhyceChain;

/// <summary>
/// Runtime middleware that anchors critical domain events to WhyceChain.
///
/// Integration point in the pipeline:
///   Event persisted (EventStore) → ChainWriter invoked → block written → THEN Kafka publish
///
/// FAILURE to write block → FAIL the command. No async inconsistency.
/// Chain write is SYNCHRONOUS with the event commit.
///
/// Anchor sets:
///   - Identity: IdentityChainAnchor.MustAnchor (identity mutations)
///   - Ledger:   LedgerChainAnchor.MustAnchor   (ledger entries, settlements — E17.3.9 STRICT)
/// </summary>
public sealed class ChainAnchorMiddleware : IMiddleware
{
    private readonly IIdentityChainAnchor _identityAnchor;
    private readonly ILedgerChainAnchor? _ledgerAnchor;

    public ChainAnchorMiddleware(IIdentityChainAnchor identityAnchor, ILedgerChainAnchor? ledgerAnchor = null)
    {
        ArgumentNullException.ThrowIfNull(identityAnchor);
        _identityAnchor = identityAnchor;
        _ledgerAnchor = ledgerAnchor;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // Execute the inner pipeline (which persists events to EventStore)
        var result = await next(context);

        if (!result.Success)
            return result;

        // After event store persist, before Kafka publish:
        // Check if any committed events need chain anchoring
        var committedEvents = context.Get<List<CommittedEvent>>(ContextKeys.CommittedEvents);
        if (committedEvents is null || committedEvents.Count == 0)
            return result;

        foreach (var committed in committedEvents)
        {
            var anchorRequest = new AnchorRequest
            {
                EventId = committed.EventId,
                AggregateId = committed.AggregateId,
                EventType = committed.EventType,
                EventData = committed.EventData,
                PolicyDecisionHash = committed.PolicyDecisionHash,
                ExecutionHash = context.ExecutionId,
                OccurredAt = committed.OccurredAt
            };

            // Identity anchor — SYNCHRONOUS, failure fails the command
            if (_identityAnchor.MustAnchor(committed.EventType))
            {
                await _identityAnchor.AnchorAsync(anchorRequest, context.CancellationToken);
                continue;
            }

            // Ledger anchor — E17.3.9 STRICT, failure fails the command
            if (_ledgerAnchor is not null && _ledgerAnchor.MustAnchor(committed.EventType))
            {
                await _ledgerAnchor.AnchorAsync(anchorRequest, context.CancellationToken);
            }
        }

        return result;
    }

    /// <summary>
    /// Records a committed event on the context for chain anchoring.
    /// Called by the engine dispatcher after event store persistence.
    /// </summary>
    public static void RecordCommittedEvent(CommandContext context, CommittedEvent committedEvent)
    {
        var events = context.Get<List<CommittedEvent>>(ContextKeys.CommittedEvents);
        if (events is null)
        {
            events = [];
            context.Set(ContextKeys.CommittedEvents, events);
        }
        events.Add(committedEvent);
    }

    public static class ContextKeys
    {
        public const string CommittedEvents = "WhyceChain.CommittedEvents";
    }
}

/// <summary>
/// Represents a domain event that has been committed to the event store
/// and is pending chain anchoring.
/// </summary>
public sealed record CommittedEvent
{
    public required string EventId { get; init; }
    public required string AggregateId { get; init; }
    public required string EventType { get; init; }
    public required object EventData { get; init; }
    public string? PolicyDecisionHash { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}
