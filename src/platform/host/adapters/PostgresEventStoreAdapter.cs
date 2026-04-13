using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Npgsql;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed event store. Persists domain events as JSONB rows
/// in the canonical events table (see 001_event_store.sql).
///
/// Deserialization is schema-driven via EventDeserializer
/// (no static EventTypeResolver, no per-domain Type dictionary).
/// </summary>
public sealed class PostgresEventStoreAdapter : IEventStore
{
    // phase1.5-S5.2.2 / KC-5 (EVENT-STORE-ADVISORY-LOCK-OBS-01):
    // canonical Whyce.EventStore meter exporting two histograms that
    // distinguish per-aggregate advisory-lock contention from pool
    // saturation. Pool acquisition time is already measured by
    // Whyce.Postgres.postgres.pool.acquisitions (PC-4); KC-5 adds
    // the Postgres-side advisory-lock dimension that PC-4 does not
    // and cannot see.
    //
    //   - event_store.append.advisory_lock_wait_ms : time spent
    //     waiting for pg_advisory_xact_lock(hashtext('events'),
    //     hashtext(@agg)) to return — i.e. the in-database queue
    //     of concurrent appenders for the same aggregate. A hot
    //     aggregate produces a Postgres-side queue invisible to
    //     the pool counters; this histogram makes it visible.
    //
    //   - event_store.append.hold_ms : time the per-aggregate
    //     critical section is held — from advisory lock acquired
    //     through tx.CommitAsync. Companion to wait_ms; together
    //     they let an operator decide whether the bottleneck is
    //     contention (high wait) or per-append duration (high
    //     hold). Tagged by outcome ∈ { "ok", "concurrency_conflict",
    //     "exception" }.
    //
    // Cardinality discipline: no aggregate_id tag, no event_type
    // tag — those would explode cardinality. The wait histogram
    // has zero tags; the hold histogram has one low-cardinality
    // outcome tag mirroring the PC-5 ChainAnchorService pattern.
    public static readonly Meter Meter = new("Whyce.EventStore", "1.0");
    private static readonly Histogram<double> AdvisoryLockWaitHistogram =
        Meter.CreateHistogram<double>("event_store.append.advisory_lock_wait_ms", unit: "ms");
    private static readonly Histogram<double> AppendHoldHistogram =
        Meter.CreateHistogram<double>("event_store.append.hold_ms", unit: "ms");

    // phase1.5-S5.2.2 / KC-8 (LOAD-EVENTS-OBSERVABILITY-01): replay
    // size histogram. Records, after every successful LoadEventsAsync
    // call, the number of events read from the store for one
    // aggregate's full replay. Closes K-R-04 by making the
    // unbounded-list memory pressure visible without restructuring
    // the replay path.
    //
    // Declared waiver: full streaming / paging of LoadEventsAsync is
    // explicitly deferred to a future workstream. The current
    // implementation reads the entire WHERE aggregate_id = @id
    // ORDER BY version stream into a List<object> with no LIMIT.
    // KC-8 records the size on every call so a growing aggregate
    // surfaces as a rising P95/P99 on this histogram, giving
    // operators a leading indicator before memory pressure becomes
    // load-bearing. The structural fix (an IAsyncEnumerable<object>
    // overload or a paged LoadEventsAsync(aggregateId, fromVersion,
    // maxRows) variant) requires extending the IEventStore interface
    // and is out of §5.2.2 narrow-scope. The waiver is canonical:
    // the seam is now DECLARED-OBSERVABLE rather than UNBOUNDED, and
    // a future structural workstream owns the streaming refactor.
    //
    // Cardinality: zero tags. Aggregate identity would explode
    // cardinality and the distribution itself is the operator
    // signal — a single hot aggregate shifts the tail percentiles.
    private static readonly Histogram<double> ReplaySizeHistogram =
        Meter.CreateHistogram<double>("event_store.replay_rows", unit: "rows");

    private readonly EventStoreDataSource _dataSource;
    private readonly EventDeserializer _deserializer;
    private readonly IIdGenerator _idGenerator;

    // phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): connection acquisitions
    // now flow through the declared event-store NpgsqlDataSource and the
    // PostgresPoolMetrics seam. Query and transaction logic below are
    // unchanged — only the connection lifecycle moves.
    public PostgresEventStoreAdapter(EventStoreDataSource dataSource, EventDeserializer deserializer, IIdGenerator idGenerator)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
        _deserializer = deserializer;
        _idGenerator = idGenerator;
    }

    public async Task<IReadOnlyList<object>> LoadEventsAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        var events = new List<object>();

        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);

        await using var cmd = new NpgsqlCommand(
            "SELECT event_type, payload FROM events WHERE aggregate_id = @id ORDER BY version ASC",
            conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): both the
        // ExecuteReaderAsync call and the per-row ReadAsync iteration
        // now honor the request/host-shutdown CancellationToken.
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var eventType = reader.GetString(0);
            var payload = reader.GetString(1);
            events.Add(_deserializer.DeserializeStored(eventType, payload));
        }

        // phase1.5-S5.2.2 / KC-8 (LOAD-EVENTS-OBSERVABILITY-01):
        // record the replay row count on every successful load. The
        // distribution's tail percentiles are the operator signal for
        // K-R-04 — a growing aggregate shifts P95/P99 upward long
        // before memory pressure becomes load-bearing. Structural
        // streaming/paging is explicitly waived; see the
        // ReplaySizeHistogram declaration block for the waiver
        // rationale.
        ReplaySizeHistogram.Record(events.Count);

        return events.AsReadOnly();
    }

    public async Task AppendEventsAsync(
        Guid aggregateId,
        IReadOnlyList<IEventEnvelope> envelopes,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        // phase1-gate-H8a: per-aggregate exclusive advisory lock. Two-key form
        // namespaces the lock to ('events', aggregate_id) so it cannot collide
        // with locks taken by other adapters on the same Postgres instance.
        // Auto-released on COMMIT or ROLLBACK. Concurrent appends to the SAME
        // aggregate serialize here; appends to DIFFERENT aggregates run in
        // parallel. This closes the SELECT MAX(version) → INSERT TOCTOU race
        // that the previous Read Committed implementation relied on the PK
        // collision to (accidentally) catch.
        //
        // phase1.5-S5.2.2 / KC-5 (EVENT-STORE-ADVISORY-LOCK-OBS-01):
        // measure the wait separately from pool acquisition. The
        // pg_advisory_xact_lock call blocks inside Postgres until the
        // lock is granted, so the elapsed time around ExecuteNonQueryAsync
        // is the in-database wait — distinct from the Whyce.Postgres
        // pool acquisition time captured above by OpenInstrumentedAsync.
        var lockWaitStart = Stopwatch.GetTimestamp();
        await using (var lockCmd = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtext('events'), hashtext(@agg::text))",
            conn, tx))
        {
            lockCmd.Parameters.AddWithValue("agg", aggregateId);
            await lockCmd.ExecuteNonQueryAsync(cancellationToken);
        }
        AdvisoryLockWaitHistogram.Record(
            Stopwatch.GetElapsedTime(lockWaitStart).TotalMilliseconds);

        // phase1.5-S5.2.2 / KC-5: hold timer starts immediately after
        // the advisory lock is acquired and stops in finally so every
        // exit path — success, ConcurrencyConflictException, transport
        // exception — is measured uniformly with the right outcome tag.
        var holdStart = Stopwatch.GetTimestamp();
        var outcome = "ok";
        try
        {

        // Compute current max version for this aggregate inside the transaction.
        // Combined with the H8a advisory lock above, this is the linearization
        // point for per-aggregate version assignment.
        int currentMax;
        await using (var maxCmd = new NpgsqlCommand(
            "SELECT COALESCE(MAX(version), -1) FROM events WHERE aggregate_id = @id",
            conn, tx))
        {
            maxCmd.Parameters.AddWithValue("id", aggregateId);
            var scalar = await maxCmd.ExecuteScalarAsync(cancellationToken);
            currentMax = scalar is int v ? v : Convert.ToInt32(scalar);
        }

        // phase1-gate-H8b: optimistic concurrency enforcement.
        // Sentinel -1 means "no check" — preserves the prior behavior for any
        // caller that has not yet been migrated to assert a version. When the
        // caller DOES assert a positive version and it disagrees with what the
        // store actually has, we throw a named exception BEFORE the INSERT so
        // nothing is persisted and the transaction rolls back cleanly.
        if (expectedVersion != -1 && expectedVersion != currentMax)
        {
            throw new ConcurrencyConflictException(aggregateId, expectedVersion, currentMax);
        }

        // phase1-gate-H7-H9-safe (#8): single multi-row INSERT instead of one
        // command per event. Same INSERT semantics, same parameters, same
        // determinism — collapses N round-trips into 1.
        if (envelopes.Count > 0)
        {
            var sql = new System.Text.StringBuilder(
                "INSERT INTO events (id, aggregate_id, aggregate_type, event_type, payload, version, created_at, " +
                "execution_hash, correlation_id, causation_id, policy_decision_hash, policy_version) VALUES ");

            await using var cmd = new NpgsqlCommand { Connection = conn, Transaction = tx };

            for (var i = 0; i < envelopes.Count; i++)
            {
                var version = currentMax + i + 1;
                var envelope = envelopes[i];
                var domainEvent = envelope.Payload;
                var eventType = envelope.EventType;
                var aggregateType = ExtractAggregateType(domainEvent);
                var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

                if (envelope.CorrelationId == Guid.Empty)
                    throw new InvalidOperationException($"Event {eventType} has empty CorrelationId — cannot persist without traceability.");
                if (envelope.CausationId == Guid.Empty)
                    throw new InvalidOperationException($"Event {eventType} has empty CausationId — cannot persist without traceability.");

                if (i > 0) sql.Append(", ");
                sql.Append($"(@id{i}, @agg{i}, @aggType{i}, @evtType{i}, @payload{i}::jsonb, @ver{i}, NOW(), " +
                           $"@execHash{i}, @corrId{i}, @causeId{i}, @policyHash{i}, @policyVer{i})");

                cmd.Parameters.AddWithValue($"id{i}", _idGenerator.Generate($"{aggregateId}:{version}"));
                cmd.Parameters.AddWithValue($"agg{i}", aggregateId);
                cmd.Parameters.AddWithValue($"aggType{i}", aggregateType);
                cmd.Parameters.AddWithValue($"evtType{i}", eventType);
                cmd.Parameters.AddWithValue($"payload{i}", payload);
                cmd.Parameters.AddWithValue($"ver{i}", version);
                cmd.Parameters.AddWithValue($"execHash{i}", envelope.ExecutionHash);
                cmd.Parameters.AddWithValue($"corrId{i}", envelope.CorrelationId);
                cmd.Parameters.AddWithValue($"causeId{i}", envelope.CausationId);
                cmd.Parameters.AddWithValue($"policyHash{i}", (object?)envelope.PolicyHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue($"policyVer{i}", DBNull.Value);
            }

            cmd.CommandText = sql.ToString();
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);

        }
        catch (ConcurrencyConflictException) when (outcome == "ok")
        {
            outcome = "concurrency_conflict";
            throw;
        }
        catch when (outcome == "ok")
        {
            // Any other throw inside the held section (transport
            // failure, deserialization, etc.) is recorded as
            // "exception" without swallowing — re-thrown after the
            // histogram record in finally.
            outcome = "exception";
            throw;
        }
        finally
        {
            // phase1.5-S5.2.2 / KC-5: hold_ms recorded on every exit
            // path. The advisory lock is released by the
            // BeginTransactionAsync DisposeAsync (auto-rollback) or by
            // CommitAsync above; either way the held section is
            // bounded by this finally block.
            AppendHoldHistogram.Record(
                Stopwatch.GetElapsedTime(holdStart).TotalMilliseconds,
                new KeyValuePair<string, object?>("outcome", outcome));
        }
    }

    private static string ExtractAggregateType(object domainEvent)
    {
        var ns = domainEvent.GetType().Namespace ?? string.Empty;
        var segments = ns.Split('.');
        return segments.Length > 0 ? segments[^1] : "Unknown";
    }
}
