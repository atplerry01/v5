using Npgsql;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EventStore;

/// <summary>
/// phase1-gate-H8a: validates that PostgresEventStoreAdapter.AppendEventsAsync
/// serializes concurrent appends for the SAME aggregate via the per-aggregate
/// advisory lock, while permitting concurrent appends across DIFFERENT
/// aggregates to proceed in parallel.
///
/// Requires a real Postgres instance reachable via the
/// Postgres__TestConnectionString environment variable. The test is silently
/// skipped (returns) when the variable is unset so unit-only test runs are
/// not blocked by infrastructure availability.
///
/// The test uses a fresh, deterministically-derived aggregate_id for each
/// test method so re-runs do not interact and rows from prior runs do not
/// affect assertions.
/// </summary>
public sealed class PostgresEventStoreConcurrencyTest
{
    private static readonly TestIdGenerator IdGen = new();

    private static string? ConnectionString =>
        Environment.GetEnvironmentVariable("Postgres__TestConnectionString")
        ?? Environment.GetEnvironmentVariable("Postgres__ConnectionString");

    private static bool SkipIfNoDatabase()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString)) return true;
        try
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return false;
        }
        catch
        {
            return true;
        }
    }

    [Fact]
    public async Task Concurrent_Appends_To_Same_Aggregate_Produce_No_Duplicate_Versions()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        // phase1.5-S5.2.5 / TB-1: PostgresEventStoreAdapter constructor
        // is now (EventStoreDataSource, EventDeserializer, IIdGenerator)
        // per PC-4 (POSTGRES-POOL-01). Wrap the raw connection string in
        // a fresh single-use NpgsqlDataSource for this test method.
        // Deserializer is only used by LoadEventsAsync; AppendEventsAsync ignores it.
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        var adapter = new PostgresEventStoreAdapter(
            new EventStoreDataSource(dataSource), deserializer: null!, IdGen);

        // Fresh aggregate id per run to avoid cross-run interference.
        var aggregateId = IdGen.Generate(
            $"H8a:Concurrent:{Guid.NewGuid()}");

        try
        {
            // Spawn N parallel appenders. Each tries to append a single event.
            // Without the advisory lock, this race would either (a) explode with
            // PK violations from accidental id collisions, or (b) corrupt the
            // stream with duplicate (aggregate_id, version) rows.
            const int parallelism = 8;
            var tasks = Enumerable.Range(0, parallelism)
                .Select(i => Task.Run(async () =>
                {
                    await adapter.AppendEventsAsync(
                        aggregateId,
                        RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent($"e{i}")),
                        expectedVersion: -1);
                }))
                .ToArray();

            await Task.WhenAll(tasks);

            // Read back versions directly via SQL — bypass LoadEventsAsync to
            // avoid the deserializer dependency for an unknown event type.
            var versions = await ReadVersionsAsync(connectionString, aggregateId);

            Assert.Equal(parallelism, versions.Count);
            Assert.Equal(parallelism, versions.Distinct().Count());
            Assert.Equal(Enumerable.Range(0, parallelism).ToArray(), versions.OrderBy(v => v).ToArray());
        }
        finally
        {
            await CleanupAsync(connectionString, aggregateId);
        }
    }

    [Fact]
    public async Task Appends_To_Different_Aggregates_Are_Not_Blocked_By_Each_Other()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        // phase1.5-S5.2.5 / TB-1: see the matching block in the first
        // test method for the EventStoreDataSource wrapping rationale.
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        var adapter = new PostgresEventStoreAdapter(
            new EventStoreDataSource(dataSource), deserializer: null!, IdGen);

        var aggA = IdGen.Generate($"H8a:CrossAgg:A:{Guid.NewGuid()}");
        var aggB = IdGen.Generate($"H8a:CrossAgg:B:{Guid.NewGuid()}");

        try
        {
            // Both should succeed in parallel without serialization on a shared
            // table-level lock. We don't measure timing — we only assert that
            // both produce the expected per-aggregate version stream.
            var tA = adapter.AppendEventsAsync(
                aggA, RawEnvelopes.Wrap(aggA, new ConcurrencyProbeEvent("a0"), new ConcurrencyProbeEvent("a1")), -1);
            var tB = adapter.AppendEventsAsync(
                aggB, RawEnvelopes.Wrap(aggB, new ConcurrencyProbeEvent("b0"), new ConcurrencyProbeEvent("b1")), -1);

            await Task.WhenAll(tA, tB);

            var versionsA = await ReadVersionsAsync(connectionString, aggA);
            var versionsB = await ReadVersionsAsync(connectionString, aggB);

            Assert.Equal(new[] { 0, 1 }, versionsA.OrderBy(v => v).ToArray());
            Assert.Equal(new[] { 0, 1 }, versionsB.OrderBy(v => v).ToArray());
        }
        finally
        {
            await CleanupAsync(connectionString, aggA);
            await CleanupAsync(connectionString, aggB);
        }
    }

    [Fact]
    public async Task Stale_Writer_With_Wrong_Expected_Version_Throws_ConcurrencyConflictException()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        // phase1.5-S5.2.5 / TB-1: see the matching block in the first
        // test method for the EventStoreDataSource wrapping rationale.
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        var adapter = new PostgresEventStoreAdapter(
            new EventStoreDataSource(dataSource), deserializer: null!, IdGen);
        var aggregateId = IdGen.Generate($"H8b:Stale:{Guid.NewGuid()}");

        try
        {
            // Establish two events: stream now has versions 0 and 1.
            await adapter.AppendEventsAsync(
                aggregateId,
                RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent("v0"), new ConcurrencyProbeEvent("v1")),
                expectedVersion: -1);

            // A stale writer asserts it expected version 0 (i.e. it observed
            // an aggregate with one event) when in fact the current version
            // is 1. The adapter must throw BEFORE inserting anything.
            var ex = await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
                adapter.AppendEventsAsync(
                    aggregateId,
                    RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent("stale")),
                    expectedVersion: 0));

            Assert.Equal(aggregateId, ex.AggregateId);
            Assert.Equal(0, ex.ExpectedVersion);
            Assert.Equal(1, ex.ActualVersion);

            // Stream must be unchanged — the rejected append must not have
            // persisted any rows.
            var versions = await ReadVersionsAsync(connectionString, aggregateId);
            Assert.Equal(new[] { 0, 1 }, versions.ToArray());

            // A correct writer (expected = 1) succeeds and produces version 2.
            await adapter.AppendEventsAsync(
                aggregateId,
                RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent("v2")),
                expectedVersion: 1);
            versions = await ReadVersionsAsync(connectionString, aggregateId);
            Assert.Equal(new[] { 0, 1, 2 }, versions.ToArray());
        }
        finally
        {
            await CleanupAsync(connectionString, aggregateId);
        }
    }

    [Fact]
    public async Task ExpectedVersion_Negative_One_Bypasses_The_Check()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        // phase1.5-S5.2.5 / TB-1: see the matching block in the first
        // test method for the EventStoreDataSource wrapping rationale.
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        var adapter = new PostgresEventStoreAdapter(
            new EventStoreDataSource(dataSource), deserializer: null!, IdGen);
        var aggregateId = IdGen.Generate($"H8b:Sentinel:{Guid.NewGuid()}");

        try
        {
            // -1 sentinel: caller asserts nothing. Must succeed regardless
            // of stream state. This protects unmigrated callers from
            // breaking when H8b is rolled out.
            await adapter.AppendEventsAsync(aggregateId,
                RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent("a")), -1);
            await adapter.AppendEventsAsync(aggregateId,
                RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent("b")), -1);
            await adapter.AppendEventsAsync(aggregateId,
                RawEnvelopes.Wrap(aggregateId, new ConcurrencyProbeEvent("c")), -1);

            var versions = await ReadVersionsAsync(connectionString, aggregateId);
            Assert.Equal(new[] { 0, 1, 2 }, versions.ToArray());
        }
        finally
        {
            await CleanupAsync(connectionString, aggregateId);
        }
    }

    private static async Task<List<int>> ReadVersionsAsync(string connectionString, Guid aggregateId)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT version FROM events WHERE aggregate_id = @id ORDER BY version", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        var result = new List<int>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(reader.GetInt32(0));
        return result;
    }

    private static async Task CleanupAsync(string connectionString, Guid aggregateId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "DELETE FROM events WHERE aggregate_id = @id", conn);
            cmd.Parameters.AddWithValue("id", aggregateId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // best-effort cleanup; do not fail the test on cleanup errors
        }
    }

    /// <summary>
    /// Probe event used by concurrency tests. Namespace segment becomes the
    /// aggregate_type written by ExtractAggregateType in the adapter.
    /// </summary>
    private sealed record ConcurrencyProbeEvent(string Tag);
}
