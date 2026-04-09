using System.Diagnostics;
using System.Diagnostics.Metrics;
using Whyce.Engines.T0U.WhyceChain.Command;
using Whyce.Engines.T0U.WhyceChain.Engine;
using Whyce.Engines.T0U.WhyceChain.Hashing;
using Whyce.Engines.T0U.WhyceChain.Result;
using Whyce.Shared.Contracts.Infrastructure.Admission;
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
    // phase1.5-S5.2.1 / PC-5 (CHAIN-ANCHOR-OBS-01): canonical Whyce.Chain
    // meter exporting two histograms that distinguish the two phases of
    // every anchor call.
    //
    //   - chain.anchor.wait_ms : time spent awaiting the global semaphore
    //                            BEFORE entering the critical section.
    //   - chain.anchor.hold_ms : time spent INSIDE the critical section
    //                            until _lock.Release().
    //
    // Step B / P-B4 confirmed the held section spans both
    // _chainEngine.Anchor (in-process) and _chainAnchor.AnchorAsync
    // (external persist), so hold_ms is dominated by external I/O. The
    // two histograms together let later §5.2.x work decide whether the
    // bottleneck is contention (high wait_ms) or hold duration (high
    // hold_ms).
    //
    // Tagging is intentionally low-cardinality:
    //   outcome ∈ { "ok", "engine_failed", "exception" }
    // No correlation_id, no aggregate_id, no policy hash — those would
    // explode cardinality without serving a §5.3.x load-work need.
    public static readonly Meter Meter = new("Whyce.Chain", "1.0");
    private static readonly Histogram<double> WaitHistogram =
        Meter.CreateHistogram<double>("chain.anchor.wait_ms", unit: "ms");
    private static readonly Histogram<double> HoldHistogram =
        Meter.CreateHistogram<double>("chain.anchor.hold_ms", unit: "ms");

    private readonly WhyceChainEngine _chainEngine;
    private readonly IChainAnchor _chainAnchor;
    // phase1.5-S5.2.2 / KW-1 (CHAIN-ANCHOR-DECLARED-01): the permit
    // count is now sized from declared ChainAnchorOptions, promoting
    // the seam from DECLARED-OPAQUE (hardcoded constant) to
    // DECLARED-BOUNDED (externalised configuration). The only value
    // the current chain integrity invariant supports is 1 — see
    // ChainAnchorOptions.PermitLimit doc for the reasoning. Structural
    // restructuring of the lock (moving I/O outside the held section,
    // sharding, etc.) is explicitly deferred to a future workstream.
    private readonly SemaphoreSlim _lock;
    private readonly ChainAnchorOptions _options;
    private string _lastBlockHash = ChainHasher.ComputeGenesisHash();
    private long _lastSequence = -1;

    public ChainAnchorService(
        WhyceChainEngine chainEngine,
        IChainAnchor chainAnchor,
        ChainAnchorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.PermitLimit < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.PermitLimit,
                "ChainAnchorOptions.PermitLimit must be at least 1.");

        _chainEngine = chainEngine;
        _chainAnchor = chainAnchor;
        _options = options;
        _lock = new SemaphoreSlim(options.PermitLimit, options.PermitLimit);
    }

    public async Task<ChainAnchorResult> AnchorAsync(
        Guid correlationId,
        IReadOnlyList<object> domainEvents,
        string policyHash,
        CancellationToken cancellationToken = default)
    {
        // phase1.5-S5.2.1 / PC-5: measure wait_ms (semaphore acquisition)
        // and hold_ms (critical section) distinctly. Stopwatch is
        // sufficient — System.Diagnostics.Metrics histograms accept
        // arbitrary doubles and the cost of two GetTimestamp calls per
        // call is negligible compared to the I/O the lock guards.
        //
        // phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): the
        // semaphore wait is now bounded by ChainAnchorOptions.WaitTimeoutMs
        // and observes the request/host-shutdown CancellationToken
        // threaded down from TC-1. On timeout, ChainAnchorWaitTimeoutException
        // is thrown — the typed RETRYABLE REFUSAL that bubbles untouched
        // to the API edge handler. The wait_ms histogram still records
        // the elapsed wait so the timeout path is observable as a
        // distinct outcome from a successful acquisition.
        var waitStart = Stopwatch.GetTimestamp();
        bool acquired;
        try
        {
            acquired = await _lock.WaitAsync(_options.WaitTimeoutMs, cancellationToken);
        }
        finally
        {
            var waitElapsedMs = Stopwatch.GetElapsedTime(waitStart).TotalMilliseconds;
            WaitHistogram.Record(waitElapsedMs);
        }

        if (!acquired)
        {
            throw new ChainAnchorWaitTimeoutException(
                _options.WaitTimeoutMs, _options.RetryAfterSeconds);
        }

        var holdStart = Stopwatch.GetTimestamp();
        var outcome = "ok";
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
                outcome = "engine_failed";
                throw new InvalidOperationException(
                    $"WhyceChain anchor failed: {result.FailureReason}. Chain integrity compromised.");
            }

            // Update runtime-owned chain state
            _lastBlockHash = result.BlockHash;
            _lastSequence = nextSequence;

            // Persist to external chain store
            // phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): the
            // chain-store adapter now consumes the CancellationToken so
            // its underlying ExecuteScalarAsync / ExecuteNonQueryAsync
            // calls honor cancellation. The held permit is unchanged —
            // I/O still occurs inside the critical section per the
            // deferred structural restructuring.
            await _chainAnchor.AnchorAsync(correlationId, domainEvents, policyHash, cancellationToken);

            return result;
        }
        catch when (outcome == "ok")
        {
            // Any throw not already classified as engine_failed (e.g.
            // chain-store transport failure inside _chainAnchor.AnchorAsync)
            // is recorded as "exception" without swallowing — the throw
            // re-propagates after the histogram record in finally.
            outcome = "exception";
            throw;
        }
        finally
        {
            var holdMs = Stopwatch.GetElapsedTime(holdStart).TotalMilliseconds;
            HoldHistogram.Record(holdMs,
                new KeyValuePair<string, object?>("outcome", outcome));
            _lock.Release();
        }
    }
}
