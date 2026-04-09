using System.Collections.Concurrent;
using Npgsql;

namespace Whycespace.Tests.Integration.FailureRecovery;

/// <summary>
/// phase1.5-S5.2.6 / FR-3 (RUNTIME-CRASH-RECOVERY-01): proves that a
/// runtime worker crashing AFTER claiming a multi-row batch with
/// `SELECT ... FOR UPDATE SKIP LOCKED` releases ALL locked rows on
/// transaction rollback, and that surviving workers reprocess the
/// entire claim set without duplication or loss.
///
/// DELTA FROM MI-2 (<c>OutboxMultiInstanceSafetyTest</c>):
///
///   MI-2's `Crash_Before_Commit_Releases_Row_For_Survivor_To_Reprocess`
///   proves the SINGLE-ROW crash-recovery seam. FR-3 extends that to:
///
///     1. MULTI-ROW CLAIM: a worker locks 5 rows in one SELECT, then
///        crashes. All 5 must release together — not just the one the
///        worker was about to publish — because the transaction owns
///        every lock acquired inside it.
///     2. NO STUCK LOCKS: a fresh SELECT after rollback observes
///        ZERO held locks for the test correlation id (proven by
///        observing that all rows are claimable by survivors).
///     3. SURVIVOR DRAIN: 3 survivor workers race the recovery and
///        publish each released row exactly once. The `publishedBy`
///        ConcurrentDictionary catches a duplicate via TryAdd-false.
///
/// SCENARIO 1.3 ALIGNMENT (from §5.2.6 spec):
///
///   * "Kill worker after row lock, before commit"            ← Test 1
///   * "Rows are reprocessed"                                 ← assertion
///   * "No duplicate publish"                                 ← TryAdd
///   * "No stuck locks"                                       ← survivors
///                                                             observe
///                                                             zero claimed
///                                                             rows held
///
///   The "after event persisted, before publish" sub-scenario is
///   covered by the natural production seam: the outbox row is
///   committed by the EventFabric persist-then-enqueue sequence
///   (separate transactions per `EventFabric.cs:130-149`), so a
///   crash between persist and enqueue leaves the event in the
///   source-of-truth and the outbox row absent — recovery is
///   replay-driven, not poll-driven, and is asserted by FR-5 (chain
///   failure leaves event-store-only state).
///
/// EXECUTION REQUIREMENTS:
///   - Real Postgres reachable via `Postgres__TestConnectionString`
///     (silent skip when unset).
///   - No Kafka required.
///   - Sequenced via <see cref="OutboxSharedTableCollection"/>.
/// </summary>
[Collection(OutboxSharedTableCollection.Name)]
public sealed class RuntimeCrashRecoveryTest
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

    private static Guid FreshCorrelationId() => Guid.NewGuid();

    [Fact]
    public async Task Multi_Row_Claim_Released_On_Crash_Survivors_Reprocess_All()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        var corrId = FreshCorrelationId();
        const int rowCount = 5;

        try
        {
            await SeedPendingRowsAsync(connectionString, corrId, rowCount);

            // ── Stage 1: a "crashed" worker claims ALL 5 rows in a single
            // SELECT FOR UPDATE SKIP LOCKED, then disposes the transaction
            // WITHOUT commit. Postgres releases every lock acquired inside
            // the transaction at rollback time — not just the row that
            // would have been published next. ──
            List<Guid> claimedIds;
            await using (var crashedConn = new NpgsqlConnection(connectionString))
            {
                await crashedConn.OpenAsync();
                await using var crashedTx = await crashedConn.BeginTransactionAsync();

                claimedIds = await ClaimRowIdsAsync(
                    crashedConn, crashedTx, corrId, limit: rowCount);
                Assert.Equal(rowCount, claimedIds.Count);

                // While the lock is HELD by the crashed tx, a fresh
                // SELECT FOR UPDATE SKIP LOCKED on a different connection
                // must see ZERO claimable rows for this corrId. Proves
                // invariant 2 (multi-row lock ownership) before the
                // rollback releases anything.
                Assert.Equal(0, await CountClaimableForCorrAsync(connectionString, corrId));

                // Intentionally NO COMMIT.
            }

            // ── Stage 2: after rollback, every row is unlocked AND
            // back to 'pending'. 3 survivor workers race to drain. ──
            var publishedBy = new ConcurrentDictionary<Guid, int>();
            var survivorTasks = Enumerable.Range(0, 3)
                .Select(workerIndex => Task.Run(() =>
                    DrainAsync(connectionString, corrId, batchSize: 3, workerIndex, publishedBy)))
                .ToArray();
            await Task.WhenAll(survivorTasks);

            // No duplicates: every claimed row was published exactly once.
            Assert.Equal(rowCount, publishedBy.Count);
            foreach (var id in claimedIds)
                Assert.True(publishedBy.ContainsKey(id),
                    $"row {id} was claimed by the crashed worker but never reprocessed");

            // No loss + correct terminal state.
            Assert.Equal(rowCount, await CountByStatusAsync(connectionString, corrId, "published"));
            Assert.Equal(0, await CountByStatusAsync(connectionString, corrId, "pending"));

            // No stuck locks: after the survivor drain there is nothing
            // claimable left for this corrId.
            Assert.Equal(0, await CountClaimableForCorrAsync(connectionString, corrId));
        }
        finally
        {
            await CleanupAsync(connectionString, corrId);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Helpers — same SQL shape as MI-2 / FR-1.
    // ─────────────────────────────────────────────────────────────────

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

        var ids = await ClaimRowIdsAsync(conn, tx, corrId, batchSize);

        if (ids.Count == 0)
        {
            await tx.CommitAsync();
            return 0;
        }

        foreach (var id in ids)
        {
            if (!publishedBy.TryAdd(id, workerIndex))
            {
                throw new InvalidOperationException(
                    $"DUPLICATE PUBLISH: row {id} processed by worker " +
                    $"{publishedBy[id]} and again by worker {workerIndex}. " +
                    "FR-3 multi-row release invariant is broken.");
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

    private static async Task<List<Guid>> ClaimRowIdsAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid corrId, int limit)
    {
        await using var cmd = new NpgsqlCommand(
            """
            SELECT id
            FROM outbox
            WHERE status = 'pending' AND correlation_id = @corr
            ORDER BY created_at ASC
            LIMIT @lim
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        cmd.Parameters.AddWithValue("corr", corrId);
        cmd.Parameters.AddWithValue("lim", limit);

        var ids = new List<Guid>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            ids.Add(reader.GetGuid(0));
        return ids;
    }

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
            while (await reader.ReadAsync()) count++;

        await tx.RollbackAsync();
        return count;
    }

    private static async Task SeedPendingRowsAsync(string connectionString, Guid corrId, int count)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        for (var i = 0; i < count; i++)
        {
            // Per-row test isolation tag (same precedent as MI-2 / FR-1).
            var rowId = Guid.NewGuid();
            await using var cmd = new NpgsqlCommand(
                """
                INSERT INTO outbox
                  (id, correlation_id, event_id, aggregate_id, event_type,
                   payload, idempotency_key, topic, status, created_at)
                VALUES
                  (@id, @corr, @id, @agg, 'Fr3ProbeEvent',
                   '{}'::jsonb, @idemp, 'whyce.events.fr3-test', 'pending', NOW())
                """,
                conn);
            cmd.Parameters.AddWithValue("id", rowId);
            cmd.Parameters.AddWithValue("corr", corrId);
            cmd.Parameters.AddWithValue("agg", corrId);
            cmd.Parameters.AddWithValue("idemp", $"fr3-test:{corrId}:{i}");
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<int> CountByStatusAsync(
        string connectionString, Guid corrId, string status)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM outbox WHERE correlation_id = @corr AND status = @s", conn);
        cmd.Parameters.AddWithValue("corr", corrId);
        cmd.Parameters.AddWithValue("s", status);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
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
            // best-effort
        }
    }
}
