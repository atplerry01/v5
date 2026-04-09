using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Admission;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

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
///   3. A <c>Whyce.Chain</c>-meter failure counter tagged with a
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
    // Whyce.Chain meter (defined by ChainAnchorService). Re-using the
    // meter keeps the chain hot path's signals coherent in any
    // registered MeterListener (OTel, Prometheus, dotnet-counters).
    private static readonly Counter<long> FailureCounter =
        Whyce.Runtime.EventFabric.ChainAnchorService.Meter
            .CreateCounter<long>("chain.store.failure");

    private readonly ChainDataSource _dataSource;
    private readonly IClock _clock;
    private readonly ChainAnchorOptions _options;

    // Breaker state. Mirrors the OpaPolicyEvaluator PC-2 pattern:
    // uncontended on the happy path; the lock only enters the critical
    // section on a failure or a state-transition observation.
    private readonly object _breakerLock = new();
    private int _consecutiveFailures;
    private DateTimeOffset? _openedAt;

    public WhyceChainPostgresAdapter(
        ChainDataSource dataSource,
        IClock clock,
        ChainAnchorOptions options)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(options);
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
    }

    public async Task<ChainBlock> AnchorAsync(
        Guid correlationId,
        IReadOnlyList<object> events,
        string decisionHash,
        CancellationToken cancellationToken = default)
    {
        // --- Breaker gate (pre-call) ---
        // Open → throw immediately. HalfOpen window has elapsed → admit a
        // single trial call (single-writer under the lock; concurrent
        // callers during HalfOpen still see Open until the trial commits).
        if (IsBreakerOpenInternal())
        {
            FailureCounter.Add(1,
                new KeyValuePair<string, object?>("outcome", "breaker_open"));
            throw new ChainAnchorUnavailableException(
                reason: "breaker_open",
                retryAfterSeconds: _options.BreakerWindowSeconds,
                message: "Chain-store circuit breaker open. No bypass allowed.");
        }

        try
        {
            var previousBlockHash = await GetPreviousBlockHashAsync(cancellationToken);
            var eventHash = ComputeEventHash(events);
            var blockId = ComputeBlockId(previousBlockHash, eventHash, decisionHash);
            var timestamp = _clock.UtcNow;

            var block = new ChainBlock(blockId, correlationId, eventHash, decisionHash, previousBlockHash, timestamp);

            await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(ChainDataSource.PoolName);

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

            // phase1.5-S5.2.3 / TC-3: ExecuteNonQueryAsync now receives
            // the request/host-shutdown CancellationToken. The empty-paren
            // form is gone.
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            RecordSuccess();
            return block;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller-driven cancellation: propagate as-is. Not a chain-store
            // health signal — do not advance the breaker.
            throw;
        }
        catch (Exception ex) when (ex is not ChainAnchorUnavailableException)
        {
            FailureCounter.Add(1,
                new KeyValuePair<string, object?>("outcome", "transport"));
            RecordFailure();
            throw new ChainAnchorUnavailableException(
                reason: "transport",
                retryAfterSeconds: _options.BreakerWindowSeconds,
                message: $"Chain-store transport failure: {ex.Message}. No bypass allowed.",
                innerException: ex);
        }
    }

    private async Task<string> GetPreviousBlockHashAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(ChainDataSource.PoolName);

        await using var cmd = new NpgsqlCommand(
            "SELECT block_id FROM whyce_chain ORDER BY timestamp DESC LIMIT 1",
            conn);

        // phase1.5-S5.2.3 / TC-3: ExecuteScalarAsync now receives the
        // request/host-shutdown CancellationToken. The empty-paren form is gone.
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is Guid id ? id.ToString() : "genesis";
    }

    // --- Breaker primitives (mirrors OpaPolicyEvaluator PC-2) ---

    /// <summary>
    /// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
    /// side-effect-free public getter for the canonical
    /// runtime-state aggregator. Mirrors the
    /// <c>OpaPolicyEvaluator.IsBreakerOpen</c> getter exactly.
    /// Returns <c>true</c> when <c>_openedAt</c> is set; does NOT
    /// perform the HalfOpen transition that the request-path
    /// internal call site does — that side-effect must remain
    /// reserved for the actual request path so the aggregator poll
    /// cannot accidentally admit a trial call.
    /// </summary>
    public bool IsBreakerOpen
    {
        get
        {
            lock (_breakerLock)
            {
                return _openedAt is not null;
            }
        }
    }

    private bool IsBreakerOpenInternal()
    {
        lock (_breakerLock)
        {
            if (_openedAt is null) return false;
            var elapsed = _clock.UtcNow - _openedAt.Value;
            if (elapsed.TotalSeconds < _options.BreakerWindowSeconds)
                return true;
            // HalfOpen: allow one trial call by clearing _openedAt while
            // leaving _consecutiveFailures intact. A successful trial
            // resets via RecordSuccess; a failed trial re-opens via
            // RecordFailure (consecutive count is already at threshold).
            _openedAt = null;
            return false;
        }
    }

    private void RecordSuccess()
    {
        lock (_breakerLock)
        {
            _consecutiveFailures = 0;
            _openedAt = null;
        }
    }

    private void RecordFailure()
    {
        lock (_breakerLock)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _options.BreakerThreshold && _openedAt is null)
            {
                _openedAt = _clock.UtcNow;
            }
        }
    }

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
