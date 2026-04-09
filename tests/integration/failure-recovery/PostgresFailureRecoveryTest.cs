using Npgsql;

namespace Whycespace.Tests.Integration.FailureRecovery;

/// <summary>
/// phase1.5-S5.2.6 / FR-2 (POSTGRES-FAILURE-RECOVERY-01): proves that
/// Postgres-side failures during outbox enqueue produce zero partial
/// state and that recovery via re-enqueue is exactly-once safe.
///
/// SCOPE WITHIN THE PHASE 1.5 §5.2.6 RE-OPEN AMENDMENT:
///
///   Failure modes under test:
///     - Connection drop / process death between INSERT and COMMIT
///       (simulated by `await using` tx disposal without commit).
///     - Multi-row batch atomicity: a half-inserted batch must rollback
///       to ZERO rows, never to a partial set.
///     - Idempotent re-enqueue via `idempotency_key`: a retried INSERT
///       with the same key is silently absorbed by
///       `ON CONFLICT (idempotency_key) DO NOTHING` — no duplicate row,
///       no thrown exception.
///
///   Acceptance criteria:
///     A1 No partial writes — abort mid-batch leaves ZERO rows.
///     A2 No data loss      — recovery re-enqueue produces every row
///                            in `pending` exactly once.
///     A3 No duplicates     — same idempotency_key replayed N times
///                            still yields exactly one row.
///
/// WHY THIS FILE EXERCISES THE OUTBOX-TABLE SQL CONTRACT DIRECTLY:
///
///   The transactional outbox guarantee that protects FR-2 lives at
///   the database level — `BEGIN; INSERT...; INSERT...; COMMIT;` with
///   automatic rollback on connection death. The C# `PostgresOutboxAdapter`
///   wrapper around that SQL is correct iff the SQL contract is correct.
///   Driving raw SQL via <see cref="NpgsqlConnection"/> lets us:
///
///     1. Pin the EXACT pattern that owns the guarantee.
///     2. Deterministically simulate "connection dropped before commit"
///        by disposing the transaction without `CommitAsync`.
///     3. Avoid the test brittleness of trying to abort an in-flight
///        adapter call via cancellation tokens, which races with the
///        adapter's own internal exception handling.
///
///   This mirrors the rationale recorded in
///   <c>OutboxMultiInstanceSafetyTest</c> for the MI-2 / FR-1 pattern.
///
/// EXECUTION REQUIREMENTS:
///
///   - Real Postgres reachable via `Postgres__TestConnectionString`
///     (silent skip when unset, identical convention to MI-2 / FR-1).
///   - No Kafka required.
///   - Tagged into <see cref="OutboxSharedTableCollection"/> so it
///     runs sequentially with MI-2 / FR-1 / FR-3 against the shared
///     `outbox` table.
/// </summary>
[Collection(OutboxSharedTableCollection.Name)]
public sealed class PostgresFailureRecoveryTest
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

    // Per-test correlation tag. Same isolation precedent as MI-2 / FR-1:
    // Guid.NewGuid here is a TEST-LEVEL isolation key, not production state
    // — production identity is deterministic via PostgresOutboxAdapter.
    private static Guid FreshCorrelationId() => Guid.NewGuid();

    [Fact]
    public async Task Connection_Drop_Mid_Batch_Rollbacks_To_Zero_Rows()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        var corrId = FreshCorrelationId();

        try
        {
            // ── Stage 1: open a tx, INSERT 5 outbox rows, then dispose
            // WITHOUT commit. This simulates a process crash, network
            // drop, or cancellation between INSERT and COMMIT — exactly
            // the failure mode FR-2 must prove safe. ──
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();

                for (var i = 0; i < 5; i++)
                {
                    await using var cmd = new NpgsqlCommand(
                        """
                        INSERT INTO outbox
                          (id, correlation_id, event_id, aggregate_id, event_type,
                           payload, idempotency_key, topic, status, created_at)
                        VALUES
                          (@id, @corr, @id, @agg, 'Fr2ProbeEvent',
                           '{}'::jsonb, @idemp, 'whyce.events.fr2-test', 'pending', NOW())
                        """,
                        conn, tx);
                    cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("corr", corrId);
                    cmd.Parameters.AddWithValue("agg", corrId);
                    cmd.Parameters.AddWithValue("idemp", $"fr2-rollback:{corrId}:{i}");
                    await cmd.ExecuteNonQueryAsync();
                }

                // Intentionally NO COMMIT. The `await using` disposal of
                // `tx` rolls back — exactly the recovery seam under test.
            }

            // A1: zero rows for this corrId. The mid-batch INSERTs were
            // rolled back atomically; nothing partially survived.
            Assert.Equal(0, await CountByCorrelationAsync(connectionString, corrId));
        }
        finally
        {
            await CleanupAsync(connectionString, corrId);
        }
    }

    [Fact]
    public async Task Recovery_After_Rollback_Reinserts_Exactly_Once()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ConnectionString!;
        var corrId = FreshCorrelationId();
        const int rowCount = 4;

        try
        {
            // Stage 1: aborted attempt — same idempotency_key sequence as
            // the recovery attempt below, but rolled back without commit.
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                for (var i = 0; i < rowCount; i++)
                    await InsertRowAsync(conn, tx, corrId, i);
                // no commit
            }
            Assert.Equal(0, await CountByCorrelationAsync(connectionString, corrId));

            // Stage 2: successful recovery — replay the same INSERTs in a
            // committed tx.
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                for (var i = 0; i < rowCount; i++)
                    await InsertRowAsync(conn, tx, corrId, i);
                await tx.CommitAsync();
            }

            // A2: every row recovered. A3: each idempotency_key produced
            // exactly one row.
            Assert.Equal(rowCount, await CountByCorrelationAsync(connectionString, corrId));

            // Stage 3: replay AGAIN in a fresh committed tx — every INSERT
            // is absorbed by ON CONFLICT (idempotency_key) DO NOTHING. The
            // row count must NOT increase.
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                for (var i = 0; i < rowCount; i++)
                    await InsertRowAsync(conn, tx, corrId, i);
                await tx.CommitAsync();
            }

            // A3: idempotent — count is still rowCount, never 2*rowCount.
            Assert.Equal(rowCount, await CountByCorrelationAsync(connectionString, corrId));
        }
        finally
        {
            await CleanupAsync(connectionString, corrId);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// INSERT one outbox row using a deterministic idempotency_key based
    /// on the test correlation id and the row index. Mirrors the
    /// production <c>ON CONFLICT (idempotency_key) DO NOTHING</c> shape
    /// so the idempotency contract is exercised end-to-end.
    /// </summary>
    private static async Task InsertRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid corrId, int index)
    {
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO outbox
              (id, correlation_id, event_id, aggregate_id, event_type,
               payload, idempotency_key, topic, status, created_at)
            VALUES
              (@id, @corr, @id, @agg, 'Fr2ProbeEvent',
               '{}'::jsonb, @idemp, 'whyce.events.fr2-test', 'pending', NOW())
            ON CONFLICT (idempotency_key) DO NOTHING
            """,
            conn, tx);
        cmd.Parameters.AddWithValue("id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("corr", corrId);
        cmd.Parameters.AddWithValue("agg", corrId);
        cmd.Parameters.AddWithValue("idemp", $"fr2-recovery:{corrId}:{index}");
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<int> CountByCorrelationAsync(string connectionString, Guid corrId)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM outbox WHERE correlation_id = @corr", conn);
        cmd.Parameters.AddWithValue("corr", corrId);
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
            // best-effort cleanup
        }
    }
}
