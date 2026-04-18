using Npgsql;
using Whycespace.Projections.Economic.Revenue.Contract;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Revenue.Contract;

/// <summary>
/// Phase 3.6 T3.6.3 — proves the parties backfill end-to-end against a real
/// projections database:
///   * Seed a legacy contract row whose JSONB state has no `parties` key.
///   * Confirm the resolver fails-fast with the explicit T3.6.4 message.
///   * Apply the backfill UPDATE (the same SQL the runbook script emits,
///     against a directly-seeded staging table so the test does not require
///     `dblink` to the event store).
///   * Confirm the resolver now returns the seeded allocations.
///   * Confirm the validation query reports zero offending rows for the
///     seeded contract id.
///
/// Skipped when no projections database is reachable
/// (REVENUE_BACKFILL_TEST_PROJECTIONS_CONN env var). This pattern matches
/// PostgresEventStoreConcurrencyTest so unit-only runs stay green without
/// a live stack.
/// </summary>
public sealed class ContractPartiesBackfillTest
{
    private static readonly TestIdGenerator IdGen = new();

    private static string? ProjectionsConnectionString =>
        Environment.GetEnvironmentVariable("REVENUE_BACKFILL_TEST_PROJECTIONS_CONN")
        ?? Environment.GetEnvironmentVariable("Postgres__ProjectionsConnectionString");

    private static bool SkipIfNoDatabase()
    {
        if (string.IsNullOrWhiteSpace(ProjectionsConnectionString)) return true;
        try
        {
            using var conn = new NpgsqlConnection(ProjectionsConnectionString);
            conn.Open();
            return false;
        }
        catch
        {
            return true;
        }
    }

    [Fact]
    public async Task LegacyContract_BeforeBackfill_FailsFast_ThenSucceedsAfterBackfill()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ProjectionsConnectionString!;
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        var contractId = IdGen.Generate("Backfill:Test:contract");
        var partyA = IdGen.Generate("Backfill:Test:partyA");
        var partyB = IdGen.Generate("Backfill:Test:partyB");

        try
        {
            // ── 1. Seed a legacy contract row WITHOUT a parties key. ───────
            await using (var seed = dataSource.CreateCommand(@"
                INSERT INTO projection_economic_revenue_contract.revenue_contract_read_model
                    (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, idempotency_key)
                VALUES
                    (@id, 'Contract', 1,
                     jsonb_build_object('contractId', @id, 'status', 'Active', 'partyCount', 2),
                     gen_random_uuid(), 'RevenueContractCreatedEvent', gen_random_uuid(), @id::text || ':seed')
                ON CONFLICT (aggregate_id) DO UPDATE SET
                    state = EXCLUDED.state"))
            {
                seed.Parameters.AddWithValue("id", contractId);
                await seed.ExecuteNonQueryAsync();
            }

            // ── 2. Resolver must fail-fast (T3.6.4) — explicit pointer to
            //       the backfill script, not a silent empty list. ───────────
            var resolver = new ContractAllocationsResolver(dataSource);
            var preBackfillEx = await Assert.ThrowsAsync<InvalidOperationException>(
                () => resolver.ResolveAsync(contractId));
            Assert.Contains("backfill", preBackfillEx.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("001_backfill_parties.sql", preBackfillEx.Message);

            // ── 3. Apply the backfill UPDATE clause directly. The runbook
            //       script normally pulls source rows via dblink; here we
            //       seed the parties JSON directly so the test does not
            //       depend on a dblink-capable cluster. The UPDATE clause
            //       below is byte-identical to 001_backfill_parties.sql. ───
            await using (var backfill = dataSource.CreateCommand(@"
                WITH targets AS (
                    SELECT rm.aggregate_id,
                           jsonb_build_array(
                             jsonb_build_object('partyId', @partyA::text, 'sharePercentage', 60),
                             jsonb_build_object('partyId', @partyB::text, 'sharePercentage', 40)
                           ) AS parties
                    FROM projection_economic_revenue_contract.revenue_contract_read_model AS rm
                    WHERE rm.aggregate_id = @id
                      AND (NOT (rm.state ? 'parties')
                           OR rm.state->'parties' IS NULL
                           OR jsonb_typeof(rm.state->'parties') = 'null'
                           OR (jsonb_typeof(rm.state->'parties') = 'array'
                               AND jsonb_array_length(rm.state->'parties') = 0))
                )
                UPDATE projection_economic_revenue_contract.revenue_contract_read_model AS rm
                SET state = jsonb_set(rm.state, '{parties}', t.parties, true),
                    projected_at = NOW()
                FROM targets t
                WHERE rm.aggregate_id = t.aggregate_id"))
            {
                backfill.Parameters.AddWithValue("id", contractId);
                backfill.Parameters.AddWithValue("partyA", partyA);
                backfill.Parameters.AddWithValue("partyB", partyB);
                await backfill.ExecuteNonQueryAsync();
            }

            // ── 4. Resolver now returns the seeded allocations. ────────────
            var allocations = await resolver.ResolveAsync(contractId);
            Assert.Equal(2, allocations.Count);
            Assert.Contains(allocations, a => a.ParticipantId == partyA.ToString() && a.OwnershipPercentage == 60m);
            Assert.Contains(allocations, a => a.ParticipantId == partyB.ToString() && a.OwnershipPercentage == 40m);

            // ── 5. Validation query returns zero offending rows for this id. ─
            await using (var validate = dataSource.CreateCommand(@"
                SELECT COUNT(*)
                FROM projection_economic_revenue_contract.revenue_contract_read_model
                WHERE aggregate_id = @id
                  AND (NOT (state ? 'parties')
                       OR state->'parties' IS NULL
                       OR jsonb_typeof(state->'parties') = 'null'
                       OR (jsonb_typeof(state->'parties') = 'array'
                           AND jsonb_array_length(state->'parties') = 0))"))
            {
                validate.Parameters.AddWithValue("id", contractId);
                var stillMissing = (long)(await validate.ExecuteScalarAsync())!;
                Assert.Equal(0, stillMissing);
            }
        }
        finally
        {
            // Cleanup: deterministic ids mean re-runs land on the same row,
            // but leaving rows behind would hide the next failure mode.
            await using var cleanup = dataSource.CreateCommand(
                "DELETE FROM projection_economic_revenue_contract.revenue_contract_read_model WHERE aggregate_id = @id");
            cleanup.Parameters.AddWithValue("id", contractId);
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task ContractWithEmptyPartiesArray_FailsFastWithCorruptionMessage()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ProjectionsConnectionString!;
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        var contractId = IdGen.Generate("Backfill:Test:corrupted-contract");

        try
        {
            await using (var seed = dataSource.CreateCommand(@"
                INSERT INTO projection_economic_revenue_contract.revenue_contract_read_model
                    (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, idempotency_key)
                VALUES
                    (@id, 'Contract', 1,
                     jsonb_build_object('contractId', @id, 'status', 'Active', 'partyCount', 0, 'parties', '[]'::jsonb),
                     gen_random_uuid(), 'RevenueContractCreatedEvent', gen_random_uuid(), @id::text || ':seed-empty')
                ON CONFLICT (aggregate_id) DO UPDATE SET state = EXCLUDED.state"))
            {
                seed.Parameters.AddWithValue("id", contractId);
                await seed.ExecuteNonQueryAsync();
            }

            var resolver = new ContractAllocationsResolver(dataSource);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => resolver.ResolveAsync(contractId));
            Assert.Contains("empty 'parties' array", ex.Message);
            Assert.Contains("corrupted row", ex.Message);
        }
        finally
        {
            await using var cleanup = dataSource.CreateCommand(
                "DELETE FROM projection_economic_revenue_contract.revenue_contract_read_model WHERE aggregate_id = @id");
            cleanup.Parameters.AddWithValue("id", contractId);
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task UnknownContract_ReturnsEmptyList_WithoutThrowing()
    {
        if (SkipIfNoDatabase()) return;

        var connectionString = ProjectionsConnectionString!;
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        var resolver = new ContractAllocationsResolver(dataSource);

        var unknownId = IdGen.Generate("Backfill:Test:unknown");
        var allocations = await resolver.ResolveAsync(unknownId);
        Assert.Empty(allocations);
    }
}
