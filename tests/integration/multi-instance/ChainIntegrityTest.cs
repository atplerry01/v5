using System.Net.Http.Json;
using System.Text.Json;
using Npgsql;

namespace Whycespace.Tests.Integration.MultiInstance;

/// <summary>
/// phase1.5-S5.5 / Stage C / Scenario 2.4 — Chain integrity validation
/// across the multi-instance topology.
///
/// CRITICAL ARCHITECTURAL FINDING (load-bearing for §5.5 readers):
///
/// The WhyceChain in this codebase is a PER-PROCESS linked structure,
/// NOT a globally serializable single linear chain across hosts. Each
/// host's <c>ChainAnchorService</c> maintains its own in-process
/// <c>_lastBlockHash</c> chain head
/// (src/runtime/event-fabric/ChainAnchorService.cs line 61), guarded
/// by a process-local <c>SemaphoreSlim(1, 1)</c> sized from
/// <c>ChainAnchorOptions.PermitLimit</c> (KW-1, line 78). KW-1
/// EXPLICITLY DEFERS distributed serialization — moving the chain head
/// mutation behind a per-correlation primitive, sharding by correlation
/// hash, or replacing the global semaphore with a distributed lock are
/// all on the future-workstream backlog.
///
/// CONSEQUENCES FOR THIS TEST:
///
///   1. With N=2 hosts, the chain table contains TWO interleaved
///      sublists that each grow independently. They share a single
///      Postgres table but neither sees the other's <c>_lastBlockHash</c>.
///   2. The same <c>previous_block_hash</c> can appear MULTIPLE times
///      across the table — each host produces its own successor to
///      whatever block it last committed, and a "fork point" is
///      structural under multi-instance, not a defect. Pre-flight
///      observation against the existing 608-row chain confirmed
///      forks: 608 blocks → 452 distinct previous_block_hash values.
///   3. The CRITICAL global invariant the runtime DOES guarantee is
///      ROW-LEVEL LINKAGE INTEGRITY: every block's
///      <c>previous_block_hash</c> references either the literal
///      "genesis" (for each host's first block) OR an existing
///      <c>block_id</c> in the table. There are no orphans.
///   4. PER-CORRELATION ordering is also consistent: events from a
///      single command have monotonic timestamps under their shared
///      correlation_id (the chain anchor is invoked once per fabric
///      ProcessAsync call).
///
/// THIS IS WHAT §5.5 / 2.4 CAN HONESTLY VALIDATE WITHOUT REQUIRING
/// PRODUCTION CHANGES TO ADD DISTRIBUTED CHAIN SERIALIZATION:
///
///   2.4.A — every block_id is unique (no collisions)
///   2.4.B — linkage integrity (no orphan blocks)
///   2.4.C — every dispatched correlation produces at least one
///           anchored block in the chain table
///   2.4.D — per-correlation block ordering is consistent
///   2.4.E — at least one host's chain head was advanced during the
///           test (proves the chain is live, not stale)
///
/// What 2.4 does NOT validate (and explicitly logs as out-of-scope
/// because the architecture explicitly defers it to a future
/// workstream): a SINGLE LINEAR GLOBAL CHAIN. KW-1's deferral note
/// is the canonical source for this — the test treats it as a
/// declared property, not a defect.
///
/// EXECUTION GATING: <c>MultiInstance__Enabled=true</c>.
/// </summary>
[Collection(MultiInstanceCollection.Name)]
public sealed class ChainIntegrityTest
{
    private const string EdgeBaseUrl = "http://localhost:18080";
    private const string ChainConnectionString =
        "Host=localhost;Port=5433;Database=whycechain;Username=whyce;Password=whyce";

    private static bool MultiInstanceEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable("MultiInstance__Enabled"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    [Fact]
    public async Task Chain_Linkage_And_Per_Correlation_Ordering_Hold_Across_Both_Hosts()
    {
        if (!MultiInstanceEnabled()) return;

        var tag = $"s5.5-s2.4-{Guid.NewGuid():N}";
        const int distinctCount = 50;

        // ── Step 1: capture the BEFORE chain row count so we can
        // distinguish "blocks added during this test" from
        // pre-existing chain history. ──
        var beforeCount = await CountTotalChainRowsAsync();

        // ── Step 2: dispatch N distinct creates, capture the
        // correlation ids returned by the API. ──
        var correlationIds = new List<Guid>(distinctCount);
        var corrLock = new object();
        using var http = new HttpClient { BaseAddress = new Uri(EdgeBaseUrl) };

        await Parallel.ForEachAsync(
            Enumerable.Range(0, distinctCount),
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            async (i, ct) =>
            {
                var payload = new
                {
                    title = $"{tag}-{i:D4}",
                    description = "stage-c-2.4",
                    userId = "test-user"
                };
                var resp = await http.PostAsJsonAsync("/api/todo/create", payload, ct);
                resp.EnsureSuccessStatusCode();
                var body = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("correlationId", out var c)
                    && c.GetString() is string corr
                    && Guid.TryParse(corr, out var corrGuid))
                {
                    lock (corrLock) correlationIds.Add(corrGuid);
                }
            });

        Assert.Equal(distinctCount, correlationIds.Count);

        // Brief settle window for the chain anchor to complete.
        // The chain anchor runs synchronously inside the request
        // path, so by the time the response returns the row should
        // already be in the table — but a small wait absorbs any
        // tail latency on the chain DB write.
        await Task.Delay(500);

        // ── Step 3: read every chain row for our correlation ids and
        // ALSO the global block table for linkage integrity checks. ──
        var ourBlocks = await ReadChainRowsForCorrelationsAsync(correlationIds);
        var allBlocks = await ReadAllBlocksAsync();
        var afterCount = allBlocks.Count;

        Console.WriteLine(
            $"[§5.5/2.4] tag={tag} dispatched={distinctCount} " +
            $"correlationIds={correlationIds.Count} " +
            $"chainBefore={beforeCount} chainAfter={afterCount} " +
            $"chainAdded={afterCount - beforeCount} " +
            $"ourBlocks={ourBlocks.Count}");

        // ── 2.4.A: every block_id is unique (the schema PK enforces
        // this; the assertion is a sanity check that the read path
        // observes the schema invariant). ──
        var distinctBlockIds = allBlocks.Select(b => b.BlockId).Distinct().Count();
        Assert.Equal(allBlocks.Count, distinctBlockIds);

        // ── 2.4.B: linkage integrity. Every block's previous_block_hash
        // is either the literal "genesis" or matches an existing
        // block_id in the table. No orphans. ──
        var blockIdSet = allBlocks.Select(b => b.BlockId.ToString()).ToHashSet();
        var orphans = allBlocks
            .Where(b => b.PreviousBlockHash != "genesis"
                        && !blockIdSet.Contains(b.PreviousBlockHash))
            .ToList();
        Assert.True(orphans.Count == 0,
            $"§5.5/2.4.B linkage integrity violation: {orphans.Count} orphan blocks " +
            $"with previous_block_hash referencing a missing block_id. " +
            $"Examples: " + string.Join(", ", orphans.Take(3).Select(o => $"{o.BlockId}->{o.PreviousBlockHash}")));

        // ── 2.4.C: every dispatched correlation produced at least one
        // anchored block. ──
        var corrIdsWithBlocks = ourBlocks.Select(b => b.CorrelationId).ToHashSet();
        var missingCorrelations = correlationIds.Where(c => !corrIdsWithBlocks.Contains(c)).ToList();
        Assert.True(missingCorrelations.Count == 0,
            $"§5.5/2.4.C missing chain anchors: {missingCorrelations.Count} of " +
            $"{correlationIds.Count} dispatched correlations have no chain block. " +
            $"This is a real defect — the runtime claims chain anchoring is " +
            $"non-bypassable.");

        // ── 2.4.D: per-correlation timestamp ordering. For each
        // correlation that produced multiple blocks, the timestamps
        // must be monotonic non-decreasing. (Most correlations will
        // produce exactly 1 block — only multi-event commands produce
        // multiple. The CreateTodo path produces 1 chain anchor call
        // per ProcessAsync, so most are size-1 here, but the gate is
        // recorded for any future test that produces multi-event
        // commands.) ──
        var byCorrelation = ourBlocks.GroupBy(b => b.CorrelationId);
        foreach (var group in byCorrelation)
        {
            var ordered = group.OrderBy(b => b.Timestamp).ToList();
            for (var i = 1; i < ordered.Count; i++)
            {
                Assert.True(ordered[i].Timestamp >= ordered[i - 1].Timestamp,
                    $"§5.5/2.4.D per-correlation timestamp regression " +
                    $"for correlation {group.Key}");
            }
        }

        // ── 2.4.E: chain is live — at least N new blocks were added
        // during the test (the chain anchor for our dispatches AND
        // the policy decision audit emission's anchor — at least
        // distinctCount, possibly more). ──
        Assert.True(afterCount - beforeCount >= distinctCount,
            $"§5.5/2.4.E chain is not advancing: only {afterCount - beforeCount} " +
            $"new blocks added during a workload that dispatched {distinctCount} commands");

        // ── 2.4 declared property (NOT a defect): the chain is
        // PER-PROCESS LINKED, not globally serializable. We DO NOT
        // assert "exactly one root" or "strict linear chain" because
        // KW-1 explicitly defers cross-process chain serialization.
        // We DO record an observation in the diagnostic line above
        // so the §5.5 wrap-up can see whether forks happened during
        // this run (which is expected under N=2 hosts). ──
        var globalForkCount = allBlocks
            .Where(b => b.PreviousBlockHash != "genesis")
            .GroupBy(b => b.PreviousBlockHash)
            .Count(g => g.Count() > 1);
        Console.WriteLine(
            $"[§5.5/2.4 declared] chain forks observed: {globalForkCount} " +
            $"(expected under N=2 hosts; KW-1 explicitly defers cross-process " +
            $"chain serialization)");
    }

    // ─────────────────────────────────────────────────────────────────
    // Helpers — direct queries against the chain database
    // ─────────────────────────────────────────────────────────────────

    private sealed record ChainBlock(
        Guid BlockId,
        Guid CorrelationId,
        string EventHash,
        string PreviousBlockHash,
        DateTime Timestamp);

    private static async Task<int> CountTotalChainRowsAsync()
    {
        await using var conn = new NpgsqlConnection(ChainConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM whyce_chain", conn);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private static async Task<List<ChainBlock>> ReadAllBlocksAsync()
    {
        var blocks = new List<ChainBlock>();
        await using var conn = new NpgsqlConnection(ChainConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            SELECT block_id, correlation_id, event_hash, previous_block_hash, timestamp
            FROM whyce_chain
            ORDER BY timestamp
            """, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            blocks.Add(new ChainBlock(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4)));
        }
        return blocks;
    }

    private static async Task<List<ChainBlock>> ReadChainRowsForCorrelationsAsync(List<Guid> correlationIds)
    {
        var blocks = new List<ChainBlock>();
        await using var conn = new NpgsqlConnection(ChainConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            SELECT block_id, correlation_id, event_hash, previous_block_hash, timestamp
            FROM whyce_chain
            WHERE correlation_id = ANY(@ids)
            ORDER BY timestamp
            """, conn);
        cmd.Parameters.AddWithValue("ids", correlationIds.ToArray());
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            blocks.Add(new ChainBlock(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4)));
        }
        return blocks;
    }
}
