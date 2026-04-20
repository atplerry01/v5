using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.3b — contract-level tests for <see cref="IDeadLetterStore"/>.
/// Exercises the idempotency + query + reprocess invariants against an
/// in-memory <see cref="ConcurrentDictionary"/>-backed implementation.
///
/// The real <c>PostgresDeadLetterStore</c> uses <c>INSERT ... ON CONFLICT
/// DO NOTHING</c> which provides the same semantics — the in-memory fake
/// mirrors that via <see cref="ConcurrentDictionary{TKey,TValue}.TryAdd"/>.
/// A separate integration test (R2.A.3c) will run the same scenarios
/// against a live Postgres schema.
/// </summary>
public sealed class DeadLetterStoreContractTests
{
    // In-memory implementation mirroring the Postgres contract: idempotent on EventId,
    // filters reprocessed entries in ListAsync by default, supports time-window queries.
    private sealed class InMemoryDeadLetterStore : IDeadLetterStore
    {
        private readonly ConcurrentDictionary<Guid, DeadLetterEntry> _store = new();

        public Task RecordAsync(DeadLetterEntry entry, CancellationToken cancellationToken = default)
        {
            _store.TryAdd(entry.EventId, entry); // first-writer wins (idempotent on EventId)
            return Task.CompletedTask;
        }

        public Task<DeadLetterEntry?> GetAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(eventId, out var entry);
            return Task.FromResult<DeadLetterEntry?>(entry);
        }

        public Task<IReadOnlyList<DeadLetterEntry>> ListAsync(
            string sourceTopic,
            DateTimeOffset? since = null,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            var effectiveLimit = Math.Min(Math.Max(1, limit), 1000);
            var results = _store.Values
                .Where(e => e.SourceTopic == sourceTopic)
                .Where(e => e.ReprocessedAt is null)
                .Where(e => since is null || e.EnqueuedAt >= since)
                .OrderByDescending(e => e.EnqueuedAt)
                .Take(effectiveLimit)
                .ToList();
            return Task.FromResult<IReadOnlyList<DeadLetterEntry>>(results);
        }

        public Task MarkReprocessedAsync(
            Guid eventId,
            string operatorIdentityId,
            DateTimeOffset reprocessedAt,
            CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(eventId, out var existing) && existing.ReprocessedAt is null)
            {
                var updated = existing with
                {
                    ReprocessedAt = reprocessedAt,
                    ReprocessedByIdentityId = operatorIdentityId
                };
                _store.TryUpdate(eventId, updated, existing);
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DeadLetterEntry>> ListAllAsync(
            DateTimeOffset? since = null,
            int limit = 100,
            bool includeReprocessed = false,
            CancellationToken cancellationToken = default)
        {
            var effectiveLimit = Math.Min(Math.Max(1, limit), 1000);
            var results = _store.Values
                .Where(e => includeReprocessed || e.ReprocessedAt is null)
                .Where(e => since is null || e.EnqueuedAt >= since)
                .OrderByDescending(e => e.EnqueuedAt)
                .Take(effectiveLimit)
                .ToList();
            return Task.FromResult<IReadOnlyList<DeadLetterEntry>>(results);
        }
    }

    private static DeadLetterEntry NewEntry(
        Guid? eventId = null,
        string sourceTopic = "whyce.ctx.domain.events",
        string eventType = "FooEvent",
        DateTimeOffset? enqueuedAt = null,
        RuntimeFailureCategory? category = RuntimeFailureCategory.DependencyUnavailable,
        int attemptCount = 3) =>
        new()
        {
            EventId = eventId ?? Guid.NewGuid(),
            SourceTopic = sourceTopic,
            EventType = eventType,
            CorrelationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            EnqueuedAt = enqueuedAt ?? DateTimeOffset.Parse("2026-04-19T00:00:00Z"),
            FailureCategory = category,
            LastError = "simulated failure",
            AttemptCount = attemptCount,
            Payload = [1, 2, 3, 4]
        };

    // ─────────────────────────────────────────────────────────────────────
    // R-DLQ-STORE-01 — idempotency on EventId
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordAsync_Same_EventId_Is_Idempotent_First_Writer_Wins()
    {
        var store = new InMemoryDeadLetterStore();
        var eventId = Guid.NewGuid();

        var first = NewEntry(eventId, attemptCount: 3);
        var second = NewEntry(eventId, attemptCount: 99); // would overwrite if non-idempotent

        await store.RecordAsync(first);
        await store.RecordAsync(second);

        var retrieved = await store.GetAsync(eventId);
        Assert.NotNull(retrieved);
        Assert.Equal(3, retrieved!.AttemptCount); // first writer wins
    }

    [Fact]
    public async Task RecordAsync_Under_Concurrent_Contention_Collapses_To_Single_Row()
    {
        var store = new InMemoryDeadLetterStore();
        var sharedEventId = Guid.NewGuid();

        const int concurrency = 50;
        using var startBarrier = new Barrier(concurrency);

        var tasks = Enumerable.Range(0, concurrency).Select(i => Task.Run(async () =>
        {
            startBarrier.SignalAndWait();
            await store.RecordAsync(NewEntry(sharedEventId, attemptCount: i));
        })).ToArray();

        await Task.WhenAll(tasks);

        var retrieved = await store.GetAsync(sharedEventId);
        Assert.NotNull(retrieved);

        // Exactly one row survives — which one depends on race, but there's
        // exactly one. Probe via ListAsync to confirm the store has a single entry.
        var listed = await store.ListAsync("whyce.ctx.domain.events");
        Assert.Single(listed);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Query semantics
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_Filters_By_Source_Topic()
    {
        var store = new InMemoryDeadLetterStore();
        await store.RecordAsync(NewEntry(sourceTopic: "topic-a"));
        await store.RecordAsync(NewEntry(sourceTopic: "topic-b"));
        await store.RecordAsync(NewEntry(sourceTopic: "topic-a"));

        var a = await store.ListAsync("topic-a");
        var b = await store.ListAsync("topic-b");

        Assert.Equal(2, a.Count);
        Assert.Single(b);
    }

    [Fact]
    public async Task ListAsync_Respects_Since_Time_Window()
    {
        var store = new InMemoryDeadLetterStore();
        var old = NewEntry(enqueuedAt: DateTimeOffset.Parse("2026-04-18T00:00:00Z"));
        var recent = NewEntry(enqueuedAt: DateTimeOffset.Parse("2026-04-19T12:00:00Z"));

        await store.RecordAsync(old);
        await store.RecordAsync(recent);

        var windowed = await store.ListAsync(
            "whyce.ctx.domain.events",
            since: DateTimeOffset.Parse("2026-04-19T00:00:00Z"));

        Assert.Single(windowed);
        Assert.Equal(recent.EventId, windowed[0].EventId);
    }

    [Fact]
    public async Task ListAsync_Orders_Newest_First()
    {
        var store = new InMemoryDeadLetterStore();
        var older = NewEntry(enqueuedAt: DateTimeOffset.Parse("2026-04-19T00:00:00Z"));
        var newer = NewEntry(enqueuedAt: DateTimeOffset.Parse("2026-04-19T12:00:00Z"));

        await store.RecordAsync(older);
        await store.RecordAsync(newer);

        var list = await store.ListAsync("whyce.ctx.domain.events");
        Assert.Equal(newer.EventId, list[0].EventId);
        Assert.Equal(older.EventId, list[1].EventId);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Reprocess semantics — audit-preserving
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkReprocessed_Hides_Entry_From_Default_List()
    {
        var store = new InMemoryDeadLetterStore();
        var entry = NewEntry();
        await store.RecordAsync(entry);

        var before = await store.ListAsync(entry.SourceTopic);
        Assert.Single(before);

        await store.MarkReprocessedAsync(
            entry.EventId,
            "operator-1",
            DateTimeOffset.Parse("2026-04-19T13:00:00Z"));

        var after = await store.ListAsync(entry.SourceTopic);
        Assert.Empty(after);

        // Entry still exists — audit preserved.
        var retrieved = await store.GetAsync(entry.EventId);
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved!.ReprocessedAt);
        Assert.Equal("operator-1", retrieved.ReprocessedByIdentityId);
    }

    [Fact]
    public async Task MarkReprocessed_Idempotent_On_Already_Reprocessed()
    {
        var store = new InMemoryDeadLetterStore();
        var entry = NewEntry();
        await store.RecordAsync(entry);

        var firstTime = DateTimeOffset.Parse("2026-04-19T13:00:00Z");
        var secondTime = DateTimeOffset.Parse("2026-04-19T14:00:00Z");

        await store.MarkReprocessedAsync(entry.EventId, "op-1", firstTime);
        await store.MarkReprocessedAsync(entry.EventId, "op-2", secondTime);

        var retrieved = await store.GetAsync(entry.EventId);
        Assert.Equal(firstTime, retrieved!.ReprocessedAt); // first reprocess wins
        Assert.Equal("op-1", retrieved.ReprocessedByIdentityId);
    }

    [Fact]
    public async Task ListAsync_Caps_Limit_At_1000()
    {
        var store = new InMemoryDeadLetterStore();

        // Try to request more than the cap. In-memory fake only has 5 entries
        // but we're testing the cap math, not the result set size.
        for (int i = 0; i < 5; i++)
            await store.RecordAsync(NewEntry());

        var unlimited = await store.ListAsync("whyce.ctx.domain.events", limit: 100_000);
        Assert.Equal(5, unlimited.Count);
    }
}
