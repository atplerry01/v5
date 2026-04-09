using System.Collections.Concurrent;
using Npgsql;

namespace Whycespace.Tests.Integration.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.5 / MI-2 (OUTBOX-MULTI-INSTANCE-01): proves that the
/// existing <c>KafkaOutboxPublisher.PublishBatchAsync</c> SQL contract
/// (<c>SELECT ... FOR UPDATE SKIP LOCKED</c> + tx-scoped publish + commit)
/// delivers exactly-once publish across N runtime instances WITHOUT a
/// claim-column scheme or a reclaim sweeper.
///
/// WHY THE TESTS EXERCISE THE SQL CONTRACT DIRECTLY (not the BackgroundService):
///
///   The exactly-once-publish guarantee under MI-2 lives at the database
///   level: row locks held across the publish-and-mark-published step,
///   automatic release on tx rollback. The C# wrapper around that SQL is
///   correct iff the SQL contract is correct. By driving the SQL contract
///   directly with raw <see cref="NpgsqlConnection"/>, we:
///
///     1. Pin the exact pattern that owns the guarantee (SELECT ...
///        FOR UPDATE SKIP LOCKED, then UPDATE inside the same tx).
///     2. Can deterministically simulate a worker crash by opening a tx,
///        locking a row, then disposing without commit — something a real
///        BackgroundService cannot expose without breaking encapsulation.
///     3. Avoid the BackgroundService start/stop dance and observable
///        races in async loop scheduling.
///
///   The harness query in <see cref="ClaimAndPublishOnceAsync"/> mirrors
///   the SELECT used by <c>KafkaOutboxPublisher.PublishBatchAsync</c>
///   (excluding the failed-row backoff predicate, which is irrelevant to
///   MI-2). If that query is ever changed in the publisher in a way that
///   breaks the locking invariants, this test must fail loudly.
///
/// Mirrors the gating convention of
/// <c>PostgresEventStoreConcurrencyTest</c>: the test is silently skipped
/// when <c>Postgres__TestConnectionString</c> is unset, so unit-only test
/// runs are not blocked by infrastructure availability.
///
/// phase1.5-S5.2.6 / FR-1: tagged into the shared-outbox collection so it
/// runs sequentially with any other test that drives the same outbox
/// table (notably <c>OutboxKafkaOutageRecoveryTest</c>). Without this,
/// xUnit's class-level parallelism can interleave their seed/drain steps
/// and corrupt each other's row-state assertions.
/// </summary>
[Xunit.Collection(Whycespace.Tests.Integration.FailureRecovery.OutboxSharedTableCollection.Name)]
public sealed class OutboxMultiInstanceSafetyTest
{
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

    // Each test stamps its rows with a fresh correlation_id so concurrent
    // CI runs and prior leftovers cannot interfere with assertions.
    private static Guid FreshCorrelationId() => Guid.NewGuid();

    [Fact]
    public async Task Multi_Instance_Workers_Publish_Each_Row_Exactly_Once()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        var corrId = FreshCorrelationId();
        const int rowCount = 20;
        const int workerCount = 4;
        const int batchSize = 5;

        try
        {
            await SeedPendingRowsAsync(connectionString, corrId, rowCount);

            // Each row id, when processed, is recorded once in this map.
            // A second TryAdd for the same id would return false — i.e. a
            // duplicate publish — and the assertion below would catch it.
            var publishedBy = new ConcurrentDictionary<Guid, int>();

            var workerTasks = Enumerable.Range(0, workerCount)
                .Select(workerIndex => Task.Run(() =>
                    DrainAsync(connectionString, corrId, batchSize, workerIndex, publishedBy)))
                .ToArray();

            await Task.WhenAll(workerTasks);

            // Invariant 1: every seeded row was published exactly once.
            Assert.Equal(rowCount, publishedBy.Count);

            // Invariant 2: no row remains in 'pending' state for this corrId.
            Assert.Equal(0, await CountByStatusAsync(connectionString, corrId, "pending"));

            // Invariant 3: every row is now 'published'.
            Assert.Equal(rowCount, await CountByStatusAsync(connectionString, corrId, "published"));
        }
        finally
        {
            await CleanupAsync(connectionString, corrId);
        }
    }

    [Fact]
    public async Task Crash_Before_Commit_Releases_Row_For_Survivor_To_Reprocess()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        var corrId = FreshCorrelationId();

        try
        {
            await SeedPendingRowsAsync(connectionString, corrId, count: 1);

            // ── Stage 1: "crashed" worker. Open a tx, claim the row with
            // FOR UPDATE SKIP LOCKED, then dispose WITHOUT committing.
            // This simulates a process crash, network drop, or cancellation
            // mid-publish. Postgres releases the row lock when the tx rolls
            // back, and the row's status reverts to whatever it was at
            // SELECT time (still 'pending' here). ──
            Guid claimedRowId;
            await using (var crashedConn = new NpgsqlConnection(connectionString))
            {
                await crashedConn.OpenAsync();
                await using var crashedTx = await crashedConn.BeginTransactionAsync();

                claimedRowId = await ClaimSingleRowIdAsync(crashedConn, crashedTx, corrId);
                Assert.NotEqual(Guid.Empty, claimedRowId);

                // ── Stage 2: while the lock is HELD by the crashed tx,
                // a survivor worker that runs the same SELECT FOR UPDATE
                // SKIP LOCKED must observe ZERO claimable rows. ──
                Assert.Equal(0, await CountClaimableForCorrAsync(connectionString, corrId));

                // Intentionally NO COMMIT. The `await using` disposal of
                // the transaction below rolls it back — exactly the
                // crash-recovery seam we are proving.
            }

            // ── Stage 3: after the rollback, the row is unlocked AND
            // back to 'pending'. A survivor worker can pick it up and
            // publish it. The published-by map proves it happens exactly
            // once even if multiple survivors race the recovery. ──
            var publishedBy = new ConcurrentDictionary<Guid, int>();
            var survivorTasks = Enumerable.Range(0, 3)
                .Select(workerIndex => Task.Run(() =>
                    DrainAsync(connectionString, corrId, batchSize: 5, workerIndex, publishedBy)))
                .ToArray();
            await Task.WhenAll(survivorTasks);

            // The original row was reprocessed by exactly one survivor.
            Assert.Single(publishedBy);
            Assert.True(publishedBy.ContainsKey(claimedRowId));
            Assert.Equal(1, await CountByStatusAsync(connectionString, corrId, "published"));
            Assert.Equal(0, await CountByStatusAsync(connectionString, corrId, "pending"));
        }
        finally
        {
            await CleanupAsync(connectionString, corrId);
        }
    }

    [Fact]
    public async Task High_Concurrency_N_Workers_M_Rows_No_Duplicates_No_Loss()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        var corrId = FreshCorrelationId();
        const int rowCount = 200;
        const int workerCount = 8;
        const int batchSize = 10;

        try
        {
            await SeedPendingRowsAsync(connectionString, corrId, rowCount);

            var publishedBy = new ConcurrentDictionary<Guid, int>();

            var workerTasks = Enumerable.Range(0, workerCount)
                .Select(workerIndex => Task.Run(() =>
                    DrainAsync(connectionString, corrId, batchSize, workerIndex, publishedBy)))
                .ToArray();

            await Task.WhenAll(workerTasks);

            // No duplicates: every row id is in the dictionary at most once
            // (TryAdd false → throw inside DrainAsync → task fault would
            // surface here from WhenAll).
            Assert.Equal(rowCount, publishedBy.Count);

            // No loss: every seeded row reached 'published'.
            Assert.Equal(rowCount, await CountByStatusAsync(connectionString, corrId, "published"));
            Assert.Equal(0, await CountByStatusAsync(connectionString, corrId, "pending"));

            // No starvation: every worker did at least some work (with 200
            // rows / 8 workers / batch=10, hot loops should round-robin).
            // We don't pin a per-worker minimum because OS scheduling can
            // legitimately give one worker the entire backlog under load —
            // we only assert that more than one worker contributed, which
            // is the meaningful "concurrency actually happened" check.
            var distinctWorkers = publishedBy.Values.Distinct().Count();
            Assert.True(distinctWorkers >= 2,
                $"expected at least 2 workers to contribute under N=8/M=200, got {distinctWorkers}");
        }
        finally
        {
            await CleanupAsync(connectionString, corrId);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Test harness
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Drains all eligible rows for the given correlation id, mimicking the
    /// publisher's poll loop until SELECT returns zero rows. Each successful
    /// row "publish" is recorded in <paramref name="publishedBy"/>; a
    /// duplicate (same row id processed twice) throws immediately so the
    /// test fault is unambiguous.
    /// </summary>
    private static async Task DrainAsync(
        string connectionString,
        Guid corrId,
        int batchSize,
        int workerIndex,
        ConcurrentDictionary<Guid, int> publishedBy)
    {
        while (true)
        {
            var processed = await ClaimAndPublishOnceAsync(
                connectionString, corrId, batchSize, workerIndex, publishedBy);
            if (processed == 0) return;
        }
    }

    /// <summary>
    /// One iteration of the publisher's poll loop, restricted to a single
    /// correlation id for test isolation. Mirrors the SQL contract used by
    /// <c>KafkaOutboxPublisher.PublishBatchAsync</c>:
    ///
    ///   1. Open connection + transaction.
    ///   2. SELECT pending rows for this corrId WITH FOR UPDATE SKIP LOCKED.
    ///   3. For each row, simulate publish (record in shared map) and
    ///      UPDATE status='published' INSIDE the same transaction.
    ///   4. COMMIT.
    /// </summary>
    private static async Task<int> ClaimAndPublishOnceAsync(
        string connectionString,
        Guid corrId,
        int batchSize,
        int workerIndex,
        ConcurrentDictionary<Guid, int> publishedBy)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        await using var selectCmd = new NpgsqlCommand(
            """
            SELECT id
            FROM outbox
            WHERE status = 'pending' AND correlation_id = @corr
            ORDER BY created_at ASC
            LIMIT @lim
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        selectCmd.Parameters.AddWithValue("corr", corrId);
        selectCmd.Parameters.AddWithValue("lim", batchSize);

        var ids = new List<Guid>();
        await using (var reader = await selectCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                ids.Add(reader.GetGuid(0));
        }

        if (ids.Count == 0)
        {
            await tx.CommitAsync();
            return 0;
        }

        foreach (var id in ids)
        {
            // "Publish" — recording the row id in a shared map. TryAdd
            // returning false means the row was published a second time,
            // which is the precise failure mode MI-2 forbids. Throwing
            // here surfaces the duplicate as a faulted Task, which the
            // test method observes via WhenAll.
            if (!publishedBy.TryAdd(id, workerIndex))
            {
                throw new InvalidOperationException(
                    $"DUPLICATE PUBLISH: row {id} processed by worker " +
                    $"{publishedBy[id]} and again by worker {workerIndex}. " +
                    "FOR UPDATE SKIP LOCKED invariant is broken.");
            }

            await using var updateCmd = new NpgsqlCommand(
                "UPDATE outbox SET status='published', published_at=NOW() WHERE id=@id",
                conn, tx);
            updateCmd.Parameters.AddWithValue("id", id);
            await updateCmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
        return ids.Count;
    }

    /// <summary>
    /// Used by the crash-recovery test to claim a single row inside an
    /// externally-managed transaction so the caller can decide whether to
    /// commit or roll back.
    /// </summary>
    private static async Task<Guid> ClaimSingleRowIdAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid corrId)
    {
        await using var cmd = new NpgsqlCommand(
            """
            SELECT id
            FROM outbox
            WHERE status = 'pending' AND correlation_id = @corr
            ORDER BY created_at ASC
            LIMIT 1
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        cmd.Parameters.AddWithValue("corr", corrId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? reader.GetGuid(0) : Guid.Empty;
    }

    /// <summary>
    /// Counts how many rows for this corrId would be claimable by a fresh
    /// SELECT FOR UPDATE SKIP LOCKED right now. Uses a separate connection
    /// + tx (rolled back at scope exit) so it does not perturb whichever
    /// transaction is currently holding the locks under test.
    /// </summary>
    private static async Task<int> CountClaimableForCorrAsync(string connectionString, Guid corrId)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        await using var cmd = new NpgsqlCommand(
            """
            SELECT id
            FROM outbox
            WHERE status = 'pending' AND correlation_id = @corr
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        cmd.Parameters.AddWithValue("corr", corrId);

        var count = 0;
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync()) count++;
        }
        await tx.RollbackAsync();
        return count;
    }

    private static async Task SeedPendingRowsAsync(string connectionString, Guid corrId, int count)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        for (var i = 0; i < count; i++)
        {
            // Deterministic per-row identity within the test, namespaced by
            // the fresh per-test corrId so we never collide with prior runs.
            // Note: Guid.NewGuid() here is a TEST-LEVEL isolation tag, not
            // production state — production identity is deterministic via
            // PostgresOutboxAdapter.ComputeDeterministicId.
            var rowId = Guid.NewGuid();
            var idempKey = $"mi2-test:{corrId}:{i}";

            await using var cmd = new NpgsqlCommand(
                """
                INSERT INTO outbox
                  (id, correlation_id, event_id, aggregate_id, event_type,
                   payload, idempotency_key, topic, status, created_at)
                VALUES
                  (@id, @corr, @id, @agg, 'Mi2ProbeEvent',
                   '{}'::jsonb, @idemp, 'whyce.events.mi2-test', 'pending', NOW())
                """,
                conn);
            cmd.Parameters.AddWithValue("id", rowId);
            cmd.Parameters.AddWithValue("corr", corrId);
            cmd.Parameters.AddWithValue("agg", corrId); // any non-empty Guid
            cmd.Parameters.AddWithValue("idemp", idempKey);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<int> CountByStatusAsync(
        string connectionString, Guid corrId, string status)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM outbox WHERE correlation_id = @corr AND status = @s",
            conn);
        cmd.Parameters.AddWithValue("corr", corrId);
        cmd.Parameters.AddWithValue("s", status);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static async Task CleanupAsync(string connectionString, Guid corrId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "DELETE FROM outbox WHERE correlation_id = @corr", conn);
            cmd.Parameters.AddWithValue("corr", corrId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // best-effort cleanup; do not fail the test on cleanup errors
        }
    }
}
