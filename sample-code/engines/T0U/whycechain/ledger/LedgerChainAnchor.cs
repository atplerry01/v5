using System.Text.Json;
using Whycespace.Engines.T0U.WhyceChain.Write;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Utilities;

namespace Whycespace.Engines.T0U.WhyceChain.Ledger;

/// <summary>
/// Anchors ledger-critical and settlement-critical domain events to the WhyceChain.
/// Only events in the anchor set are written to the immutable ledger.
///
/// E17.3.9: STRICT mode — anchor failure FAILS the operation.
///
/// Anchored events:
///   ledger.entry.recorded,
///   settlement.created, settlement.completed,
///   revenue.recognized, revenue.reversed,
///   distribution.executed, distribution.clawback
/// </summary>
public sealed class LedgerChainAnchor : ChainEngineBase, ILedgerChainAnchor
{
    private readonly IChainWriter _writer;

    private static readonly HashSet<string> AnchoredEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Ledger events (STRICT)
        "ledger.entry.recorded",
        "LedgerEntryRecordedEvent",

        // Settlement events (STRICT — E17.4.7)
        "settlement.created",
        "SettlementCreatedEvent",
        "settlement.completed",
        "SettlementCompletedEvent",
        "settlement.executed",

        // Revenue events (STRICT — E17.5.7)
        "revenue.recognized",
        "RevenueRecognizedEvent",
        "revenue.reversed",
        "RevenueReversedEvent",

        // Distribution events (STRICT — E17.6.7)
        "distribution.executed",
        "DistributionExecutedEvent",
        "distribution.clawback",
        "DistributionClawbackEvent",

        // Cluster structure events (STRICT — E18.1-E18.4)
        "cluster.created",
        "ClusterCreatedEvent",
        "cluster.activated",
        "ClusterActivatedEvent",
        "authority.created",
        "AuthorityCreatedEvent",
        "subcluster.created",
        "SubClusterCreatedEvent",
        "spv.created",
        "SpvCreatedEvent",
        "spv.activated",
        "SpvActivatedEvent",
        "spv.operator.replaced",
        "SpvOperatorReplacedEvent",

        // Cross-SPV transaction events (STRICT — E18.5)
        "crossspv.created",
        "CrossSpvCreatedEvent",
        "crossspv.prepared",
        "CrossSpvPreparedEvent",
        "crossspv.committed",
        "CrossSpvCommittedEvent",
        "crossspv.failed",
        "CrossSpvFailedEvent",
        "crossspv.state.changed",
        "CrossSpvStateChangedEvent",

        // Cluster governance decision events (STRICT — E18.7)
        "governance.decision.proposed",
        "GovernanceDecisionProposedEvent",
        "governance.decision.approved",
        "GovernanceDecisionApprovedEvent",
        "governance.decision.executed",
        "GovernanceDecisionExecutedEvent",
        "governance.decision.rejected",
        "GovernanceDecisionRejectedEvent",
    };

    public LedgerChainAnchor(IChainWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>
    /// Returns true if this event type MUST be anchored to the chain.
    /// </summary>
    public static bool MustAnchorStatic(string eventType)
    {
        return AnchoredEventTypes.Contains(eventType);
    }

    /// <inheritdoc />
    bool ILedgerChainAnchor.MustAnchor(string eventType)
    {
        return AnchoredEventTypes.Contains(eventType);
    }

    /// <summary>
    /// Anchor a ledger-critical event to the WhyceChain.
    /// Computes payload hash from event data and writes an immutable block.
    ///
    /// STRICT: failure to anchor → command MUST fail. No async inconsistency.
    /// </summary>
    public async Task<ChainWriteResult> AnchorAsync(
        AnchorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!MustAnchorStatic(request.EventType))
            throw new InvalidOperationException(
                $"Event type '{request.EventType}' is not in the ledger anchor set.");

        var eventDataHash = HashUtility.ComputeSha256(
            JsonSerializer.Serialize(request.EventData, ChainSerializerOptions.Default));

        var payload = new ChainPayload
        {
            EventId = request.EventId,
            AggregateId = request.AggregateId,
            EventType = request.EventType,
            EventDataHash = eventDataHash,
            PolicyDecisionHash = request.PolicyDecisionHash,
            ExecutionHash = request.ExecutionHash,
            OccurredAt = request.OccurredAt
        };

        return await _writer.WriteAsync(payload, cancellationToken);
    }
}