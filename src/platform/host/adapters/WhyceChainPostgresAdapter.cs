using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Infrastructure.Admission;
using Whycespace.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed chain anchor. Persists immutable chain blocks to the
/// whyce_chain table. Each block references the previous block hash,
/// forming an append-only integrity chain.
///
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): connection lifecycle is
/// owned by the declared chain pool, logically distinct from the
/// event-store pool so a chain-store outage cannot exhaust event-store
/// connections.
///
/// phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): hardened against
/// the pre-S5.2.3 unbounded I/O shape (no token threading, no breaker,
/// no failure observability). Three guarantees are added without
/// changing chain semantics:
///
///   1. The request/host-shutdown <see cref="CancellationToken"/> now
///      reaches every <c>ExecuteScalarAsync</c> / <c>ExecuteNonQueryAsync</c>
///      call. The empty-paren forms are gone.
///   2. A narrow consecutive-failure circuit breaker (Closed / Open /
///      HalfOpen) sized from
///      <see cref="ChainAnchorOptions.BreakerThreshold"/> and
///      <see cref="ChainAnchorOptions.BreakerWindowSeconds"/>. Open-state
///      calls throw <see cref="ChainAnchorUnavailableException"/>
///      immediately, never silently allow.
///   3. A <c>Whycespace.Chain</c>-meter failure counter tagged with a
///      low-cardinality outcome ("breaker_open" / "transport") so a
///      degraded chain-store is observable end-to-end without
///      exploding cardinality.
///
/// Failure semantics: transport failures and breaker-open both surface as
/// <see cref="ChainAnchorUnavailableException"/> — the typed RETRYABLE
/// REFUSAL counterpart to <see cref="ChainAnchorWaitTimeoutException"/>.
/// Caller-driven cancellation propagates as <see cref="OperationCanceledException"/>
/// without wrapping so host shutdown semantics are preserved.
/// </summary>
public sealed class WhyceChainPostgresAdapter : IChainAnchor
{
    // phase1.5-S5.2.3 / TC-3: failure counter on the existing
    // Whycespace.Chain meter (defined by ChainAnchorService). Re-using the
    // meter keeps the chain hot path's signals coherent in any
    // registered MeterListener (OTel, Prometheus, dotnet-counters).
    private static readonly Counter<long> FailureCounter =
        Whycespace.Runtime.EventFabric.ChainAnchorService.Meter
            .CreateCounter<long>("chain.store.failure");

    private readonly ChainDataSource _dataSource;
    private readonly IClock _clock;
    private readonly ChainAnchorOptions _options;

    // R2.A.D.3a / R-CHAIN-BREAKER-DELEGATION-01 — delegated to the canonical
    // ICircuitBreaker. Inline state (_breakerLock / _consecutiveFailures /
    // _openedAt) removed to match the R2.A.D.2 OPA refactor pattern. The
    // breaker INSTANCE is owned by host composition so it can be registered
    // by name ("chain-anchor") and surfaced by IRuntimeStateAggregator via
    // ICircuitBreakerRegistry.GetAll() iteration (R2.A.D.4).
    private readonly ICircuitBreaker _breaker;

    public WhyceChainPostgresAdapter(
        ChainDataSource dataSource,
        IClock clock,
        ChainAnchorOptions options,
        ICircuitBreaker breaker)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(breaker);
        if (options.BreakerThreshold < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.BreakerThreshold,
                "ChainAnchorOptions.BreakerThreshold must be at least 1.");
        if (options.BreakerWindowSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.BreakerWindowSeconds,
                "ChainAnchorOptions.BreakerWindowSeconds must be at least 1.");

        _dataSource = dataSource;
        _clock = clock;
        _options = options;
        _breaker = breaker;
    }

    public async Task<ChainBlock> AnchorAsync(
        Guid correlationId,
        IReadOnlyList<object> events,
        string decisionHash,
        CancellationToken cancellationToken = default)
    {
        // R2.A.D.3a / R-CHAIN-BREAKER-DELEGATION-01: DB work wrapped in
        // the injected circuit breaker. Transport failures inside the
        // lambda throw ChainAnchorUnavailableException — the breaker
        // counts these as failures. Successful anchor = breaker success.
        // On Open, breaker throws CircuitBreakerOpenException which we
        // translate at the adapter boundary (R-CHAIN-BREAKER-BOUNDARY-01)
        // so callers keep seeing the same typed exception.
        try
        {
            return await _breaker.ExecuteAsync<ChainBlock>(async ct =>
            {
                try
                {
                    var previousBlockHash = await GetPreviousBlockHashAsync(ct);
                    var eventHash = ComputeEventHash(events);
                    var blockId = ComputeBlockId(previousBlockHash, eventHash, decisionHash);
                    var timestamp = _clock.UtcNow;

                    var block = new ChainBlock(blockId, correlationId, eventHash, decisionHash, previousBlockHash, timestamp);

                    await using var conn = await _dataSource.OpenAsync(ct);

                    await using var cmd = new NpgsqlCommand(
                        """
                        INSERT INTO whyce_chain (block_id, correlation_id, event_hash, decision_hash, previous_block_hash, timestamp)
                        VALUES (@blockId, @corrId, @evtHash, @decHash, @prevHash, @ts)
                        """,
                        conn);

                    cmd.Parameters.AddWithValue("blockId", block.BlockId);
                    cmd.Parameters.AddWithValue("corrId", block.CorrelationId);
                    cmd.Parameters.AddWithValue("evtHash", block.EventHash);
                    cmd.Parameters.AddWithValue("decHash", block.DecisionHash);
                    cmd.Parameters.AddWithValue("prevHash", block.PreviousBlockHash);
                    cmd.Parameters.AddWithValue("ts", block.Timestamp);

                    await cmd.ExecuteNonQueryAsync(ct);

                    return block;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // Caller-driven cancellation propagates unchanged — the
                    // breaker itself honors OCE and does NOT count it as
                    // a failure, so the pre-refactor invariant holds.
                    throw;
                }
                catch (CircuitBreakerOpenException)
                {
                    // R2.A.D.3c: the postgres-pool breaker can trip while
                    // the chain-anchor breaker is Closed. Re-throw so the
                    // outer catch discriminates by BreakerName and attributes
                    // the failure correctly (pool, not chain-store transport).
                    throw;
                }
                catch (Exception ex)
                {
                    FailureCounter.Add(1,
                        new KeyValuePair<string, object?>("outcome", "transport"));
                    throw new ChainAnchorUnavailableException(
                        reason: "transport",
                        retryAfterSeconds: _options.BreakerWindowSeconds,
                        message: $"Chain-store transport failure: {ex.Message}. No bypass allowed.",
                        innerException: ex);
                }
            },
            cancellationToken);
        }
        catch (CircuitBreakerOpenException breakerEx)
        {
            // R-CHAIN-BREAKER-BOUNDARY-01: translate at the adapter edge so
            // external callers (ChainAnchorService, API-edge handler) see
            // the same typed exception shape as pre-R2.A.D.3a.
            //
            // R2.A.D.3c: distinguish chain-anchor breaker (this adapter's
            // own dependency breaker) from the shared postgres-pool breaker.
            // Both surface as ChainAnchorUnavailableException but the
            // failure-counter outcome tag differs so operator dashboards
            // attribute the root cause correctly.
            var outcomeTag = breakerEx.BreakerName == "postgres-pool"
                ? "pool_breaker_open"
                : "breaker_open";
            FailureCounter.Add(1,
                new KeyValuePair<string, object?>("outcome", outcomeTag));
            throw new ChainAnchorUnavailableException(
                reason: outcomeTag,
                retryAfterSeconds: breakerEx.RetryAfterSeconds,
                message: breakerEx.BreakerName == "postgres-pool"
                    ? "Postgres pool circuit breaker open. No bypass allowed."
                    : "Chain-store circuit breaker open. No bypass allowed.",
                innerException: breakerEx);
        }
    }

    private async Task<string> GetPreviousBlockHashAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            "SELECT block_id FROM whyce_chain ORDER BY timestamp DESC LIMIT 1",
            conn);

        // phase1.5-S5.2.3 / TC-3: ExecuteScalarAsync now receives the
        // request/host-shutdown CancellationToken. The empty-paren form is gone.
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is Guid id ? id.ToString() : "genesis";
    }

    // --- Breaker state getter (HC-2 consumer contract preserved) ---

    /// <summary>
    /// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
    /// side-effect-free public getter. Post-R2.A.D.4 the
    /// <see cref="RuntimeStateAggregator"/> reads breaker state via
    /// <c>ICircuitBreakerRegistry</c> iteration and no longer calls this
    /// getter directly. Retained for any remaining consumers with adapted
    /// semantics: returns true when the delegated
    /// <see cref="ICircuitBreaker"/> is NOT Closed.
    /// </summary>
    public bool IsBreakerOpen => _breaker.State != CircuitBreakerState.Closed;

    private static string ComputeEventHash(IReadOnlyList<object> events)
    {
        var payload = JsonSerializer.Serialize(events);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    private static Guid ComputeBlockId(string previousHash, string eventHash, string decisionHash)
    {
        var seed = $"{previousHash}:{eventHash}:{decisionHash}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}
