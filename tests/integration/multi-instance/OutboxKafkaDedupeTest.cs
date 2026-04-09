using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Confluent.Kafka;
using Npgsql;

namespace Whycespace.Tests.Integration.MultiInstance;

/// <summary>
/// phase1.5-S5.5 / Stage B / Scenario 2.2 — Outbox multi-instance
/// behavior at the system level.
///
/// SCOPE:
///
///   * Both whyce-host-1 and whyce-host-2 run their own
///     KafkaOutboxPublisher BackgroundService against the SHARED
///     Postgres outbox table.
///   * MI-2 already proved the SQL contract guarantees exactly-once
///     publish per row (FOR UPDATE SKIP LOCKED + tx-scoped publish).
///   * Stage B / 2.2 proves the SAME guarantee end-to-end at the
///     KAFKA BROKER level — i.e. that the system observable
///     downstream of both publishers contains each event-id exactly
///     once.
///
/// METHOD:
///
///   1. Pre-flight: count any pre-existing messages on the canonical
///      todo events topic (from prior smoke tests / runs) so the
///      assertion can ignore them.
///   2. Subscribe a real Kafka consumer to
///      whyce.operational.sandbox.todo.events with a fresh
///      consumer-group id (so the consumer reads from the END of the
///      topic onward, ignoring history) — actually we use
///      auto.offset.reset=latest + a unique group, then issue a
///      Subscribe + a Poll loop until partitions are assigned, then
///      record current high-water-marks as our "start" point.
///   3. Drive N distinct create requests through the edge front door.
///      Each will produce 2 events through the runtime fabric → both
///      hosts' KafkaOutboxPublisher BackgroundServices race to drain
///      them.
///   4. Consume from the topic until we have observed the expected
///      number of messages (2N) OR a timeout elapses.
///   5. Dedupe by `event-id` header. Assert no duplicates.
///   6. Cross-check with Postgres outbox: every row reached
///      `published`, none stuck in `pending` / `failed` / `deadletter`.
///
/// EXECUTION GATING: <c>MultiInstance__Enabled=true</c>.
///
/// REQUIRES: the multi-instance compose stack must be running, and
/// Kafka must be reachable at <c>localhost:29092</c> (the external
/// listener defined in the base compose).
/// </summary>
[Collection(MultiInstanceCollection.Name)]
public sealed class OutboxKafkaDedupeTest
{
    private const string EdgeBaseUrl = "http://localhost:18080";
    private const string KafkaBootstrap = "localhost:29092";
    private const string TopicName = "whyce.operational.sandbox.todo.events";
    private const string PgConnectionString =
        "Host=localhost;Port=5432;Database=whyce_eventstore;Username=whyce;Password=whyce";

    private static bool MultiInstanceEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable("MultiInstance__Enabled"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    [Fact]
    public async Task Outbox_Across_Two_Hosts_Publishes_Each_Event_Exactly_Once_To_Kafka()
    {
        if (!MultiInstanceEnabled()) return;

        // Per-test correlation tag — embedded in titles so the
        // dispatched aggregates are unique per test run, and so the
        // SQL/Kafka assertions can scope by correlation_id.
        var tag = $"s5.5-s2.2-{Guid.NewGuid():N}";
        const int distinctCommands = 50;

        // ── Step 1: build a Kafka consumer with a fresh group id and
        // start consuming from "latest" so we ignore historical messages
        // from prior runs of the smoke / harness. ──
        var groupId = $"stage-b-2.2-consumer-{Guid.NewGuid():N}";
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = KafkaBootstrap,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false,
            EnablePartitionEof = false,
            SessionTimeoutMs = 10_000,
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(TopicName);

        // Force partition assignment by polling once with a short timeout.
        // The first poll after Subscribe triggers the rebalance / assign
        // path, which is otherwise lazy. We then know we are positioned
        // at "latest" and any subsequent message produced is in scope.
        for (var i = 0; i < 10; i++)
        {
            consumer.Consume(TimeSpan.FromMilliseconds(500));
            if (consumer.Assignment.Count > 0) break;
        }
        Assert.NotEmpty(consumer.Assignment);

        // ── Step 2: dispatch N distinct creates concurrently across
        // the edge front door. Each request → 2 events (from §5.3
        // observation) → 2 outbox rows → 2 Kafka messages. ──
        var dispatchedTodoIds = new ConcurrentBag<string>();
        using var http = new HttpClient { BaseAddress = new Uri(EdgeBaseUrl) };
        await Parallel.ForEachAsync(
            Enumerable.Range(0, distinctCommands),
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            async (i, ct) =>
            {
                var payload = new
                {
                    title = $"{tag}-{i:D4}",
                    description = "stage-b-2.2",
                    userId = "test-user"
                };
                var resp = await http.PostAsJsonAsync("/api/todo/create", payload, ct);
                resp.EnsureSuccessStatusCode();
                var body = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("todoId", out var t) && t.GetString() is string id)
                {
                    dispatchedTodoIds.Add(id);
                }
            });

        Assert.Equal(distinctCommands, dispatchedTodoIds.Count);

        // ── Step 3: consume from Kafka until we have collected enough
        // messages OR a generous timeout elapses. We are looking for
        // messages whose event-id header is associated with one of our
        // dispatched aggregate ids — but the simpler shape is to
        // collect EVERY message during the window, dedupe by event-id,
        // and intersect against our dispatched id set after the fact.
        //
        // Per-message dedupe key: the `event-id` header is the
        // canonical idempotency key the publisher attaches at line 186
        // of KafkaOutboxPublisher.cs. We track:
        //   - eventIdsSeen        — every distinct event-id observed
        //   - eventIdSightings    — count of how many times each
        //                           event-id was delivered (>1 = dup)
        //   - aggregateIdsSeen    — every distinct aggregate-id header
        // ──
        var eventIdsSeen = new HashSet<string>();
        var eventIdSightings = new Dictionary<string, int>();
        var aggregateIdsSeen = new HashSet<string>();
        var messageCount = 0;

        var consumeDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < consumeDeadline)
        {
            var cr = consumer.Consume(TimeSpan.FromMilliseconds(500));
            if (cr is null || cr.Message is null) continue;

            messageCount++;
            var eventId = GetHeaderString(cr.Message.Headers, "event-id");
            var aggregateId = GetHeaderString(cr.Message.Headers, "aggregate-id");

            if (eventId is not null)
            {
                eventIdsSeen.Add(eventId);
                eventIdSightings[eventId] = eventIdSightings.GetValueOrDefault(eventId, 0) + 1;
            }
            if (aggregateId is not null)
            {
                aggregateIdsSeen.Add(aggregateId);
            }

            // Early-exit once we have observed enough messages whose
            // aggregate-id intersects our dispatched set. The runtime
            // fabric emits a deterministic 2 events per command
            // (observed in §5.3 evidence), so we expect 2 × N messages
            // tagged with our aggregate ids.
            var ourAggregateMatches = aggregateIdsSeen.Intersect(dispatchedTodoIds).Count();
            if (ourAggregateMatches >= distinctCommands)
            {
                // Drain a small additional window to catch any in-flight
                // duplicates the publisher might still be emitting.
                var drainDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
                while (DateTime.UtcNow < drainDeadline)
                {
                    var extra = consumer.Consume(TimeSpan.FromMilliseconds(200));
                    if (extra?.Message is null) continue;
                    messageCount++;
                    var extraEventId = GetHeaderString(extra.Message.Headers, "event-id");
                    var extraAggregateId = GetHeaderString(extra.Message.Headers, "aggregate-id");
                    if (extraEventId is not null)
                    {
                        eventIdsSeen.Add(extraEventId);
                        eventIdSightings[extraEventId] = eventIdSightings.GetValueOrDefault(extraEventId, 0) + 1;
                    }
                    if (extraAggregateId is not null) aggregateIdsSeen.Add(extraAggregateId);
                }
                break;
            }
        }

        consumer.Close();

        // ── Step 4: assertions. ──
        var ourMessages = eventIdSightings
            .Where(kv => kv.Value > 0)
            .ToList();
        var ourAggregateIds = aggregateIdsSeen.Intersect(dispatchedTodoIds).ToList();

        Console.WriteLine(
            $"[§5.5/2.2] tag={tag} dispatched={distinctCommands} " +
            $"messagesConsumed={messageCount} distinctEventIds={eventIdsSeen.Count} " +
            $"ourAggregateMatches={ourAggregateIds.Count}");

        // INVARIANT 1: every dispatched aggregate id appeared on Kafka.
        Assert.Equal(distinctCommands, ourAggregateIds.Count);

        // INVARIANT 2: NO event-id was delivered more than once.
        // This is the canonical "exactly-once" assertion at the system
        // level. The publisher's MarkAsPublishedAsync runs inside the
        // SELECT FOR UPDATE SKIP LOCKED transaction (MI-2), so two
        // hosts cannot both successfully publish the same row. The
        // narrow at-least-once seam is the broker-ack window — if the
        // host crashes between Kafka ack and the UPDATE COMMIT, the
        // row reverts to pending and the survivor re-publishes. We
        // do not exercise that crash window in 2.2 (Stage D / 2.5
        // does), so under steady-state we expect strict 1:1.
        var duplicates = eventIdSightings.Where(kv => kv.Value > 1).ToList();
        Assert.True(duplicates.Count == 0,
            $"§5.5/2.2 duplicate publish detected: {duplicates.Count} event-ids " +
            $"delivered more than once. Examples: " +
            string.Join(", ", duplicates.Take(5).Select(kv => $"{kv.Key}={kv.Value}")));

        // INVARIANT 3: cross-check Postgres outbox. Every row whose
        // aggregate_id is in our dispatched set must be in `published`
        // status, with zero `pending`/`failed`/`deadletter`.
        var outboxState = await ReadOutboxStateForAggregateSetAsync(dispatchedTodoIds.ToArray());
        Console.WriteLine(
            $"[§5.5/2.2] outbox post-state: pending={outboxState.Pending} " +
            $"failed={outboxState.Failed} deadletter={outboxState.Deadletter} " +
            $"published={outboxState.Published}");
        Assert.Equal(0, outboxState.Pending);
        Assert.Equal(0, outboxState.Failed);
        Assert.Equal(0, outboxState.Deadletter);
        Assert.True(outboxState.Published >= distinctCommands,
            $"§5.5/2.2 expected ≥{distinctCommands} published outbox rows, " +
            $"observed {outboxState.Published}");
    }

    // ─────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────

    private static string? GetHeaderString(Headers? headers, string name)
    {
        if (headers is null) return null;
        if (!headers.TryGetLastBytes(name, out var bytes) || bytes is null) return null;
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private readonly record struct OutboxStateSnapshot(
        int Pending, int Failed, int Deadletter, int Published);

    private static async Task<OutboxStateSnapshot> ReadOutboxStateForAggregateSetAsync(string[] aggregateIds)
    {
        var ids = aggregateIds.Select(Guid.Parse).ToArray();
        await using var conn = new NpgsqlConnection(PgConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            SELECT
                COUNT(*) FILTER (WHERE status = 'pending'),
                COUNT(*) FILTER (WHERE status = 'failed'),
                COUNT(*) FILTER (WHERE status = 'deadletter'),
                COUNT(*) FILTER (WHERE status = 'published')
            FROM outbox
            WHERE aggregate_id = ANY(@ids)
            """, conn);
        cmd.Parameters.AddWithValue("ids", ids);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new OutboxStateSnapshot(
            (int)reader.GetInt64(0),
            (int)reader.GetInt64(1),
            (int)reader.GetInt64(2),
            (int)reader.GetInt64(3));
    }
}
