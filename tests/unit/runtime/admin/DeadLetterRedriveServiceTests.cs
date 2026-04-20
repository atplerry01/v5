using System.Collections.Concurrent;
using Whycespace.Runtime.ControlPlane.Admin;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime.Admin;

/// <summary>
/// R4.B / R-ADMIN-REDRIVE-ELIGIBILITY-01 — DLQ re-drive eligibility gate:
/// NotFound, AlreadyReprocessed, Ineligible (missing topic/payload), and
/// PublishFailed branches refuse safely; Accepted path marks reprocessed +
/// emits audit.
/// </summary>
public sealed class DeadLetterRedriveServiceTests
{
    private const string Operator = "operator-alice";

    [Fact]
    public async Task Redrive_refuses_missing_entry()
    {
        var (svc, dlq, _, auditor) = Build();

        var result = await svc.RedriveAsync(Guid.Parse("11111111-0000-0000-0000-000000000001"), Operator, rationale: null);

        Assert.Equal(DeadLetterRedriveOutcome.NotFound, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.Empty(dlq.Reprocessed);
        var audit = Assert.Single(auditor.Calls);
        Assert.Equal(OperatorActionOutcomes.Refused, audit.Outcome);
    }

    [Fact]
    public async Task Redrive_refuses_already_reprocessed()
    {
        var (svc, dlq, _, auditor) = Build();
        var entry = NewEntry(id: Guid.Parse("22222222-0000-0000-0000-000000000002"));
        dlq.Record(entry);
        await dlq.MarkReprocessedAsync(entry.EventId, "prior-operator", DateTimeOffset.UtcNow);

        var result = await svc.RedriveAsync(entry.EventId, Operator, rationale: null);

        Assert.Equal(DeadLetterRedriveOutcome.AlreadyReprocessed, result.Outcome);
        var audit = Assert.Single(auditor.Calls);
        Assert.Equal(OperatorActionOutcomes.Refused, audit.Outcome);
    }

    [Fact]
    public async Task Redrive_refuses_when_payload_empty()
    {
        var (svc, dlq, _, auditor) = Build();
        var entry = NewEntry(id: Guid.Parse("33333333-0000-0000-0000-000000000003"), payload: Array.Empty<byte>());
        dlq.Record(entry);

        var result = await svc.RedriveAsync(entry.EventId, Operator, rationale: null);

        Assert.Equal(DeadLetterRedriveOutcome.Ineligible, result.Outcome);
        Assert.Empty(dlq.Reprocessed);
        var audit = Assert.Single(auditor.Calls);
        Assert.Equal(OperatorActionOutcomes.Refused, audit.Outcome);
    }

    [Fact]
    public async Task Redrive_marks_publish_failed_and_does_not_mark_reprocessed()
    {
        var (svc, dlq, publisher, auditor) = Build();
        var entry = NewEntry(id: Guid.Parse("44444444-0000-0000-0000-000000000004"));
        dlq.Record(entry);
        publisher.ShouldThrow = new DeadLetterRedrivePublishException("broker unavailable");

        var result = await svc.RedriveAsync(entry.EventId, Operator, rationale: "retry after broker recovery");

        Assert.Equal(DeadLetterRedriveOutcome.PublishFailed, result.Outcome);
        Assert.Empty(dlq.Reprocessed);
        Assert.Equal("broker unavailable", result.FailureReason);
        var audit = Assert.Single(auditor.Calls);
        Assert.Equal(OperatorActionOutcomes.Failed, audit.Outcome);
    }

    [Fact]
    public async Task Redrive_accepts_eligible_entry_marks_reprocessed_publishes_and_audits_accepted()
    {
        var (svc, dlq, publisher, auditor) = Build();
        var entry = NewEntry(id: Guid.Parse("55555555-0000-0000-0000-000000000005"));
        dlq.Record(entry);

        var result = await svc.RedriveAsync(entry.EventId, Operator, rationale: "operator-approved");

        Assert.Equal(DeadLetterRedriveOutcome.Accepted, result.Outcome);
        Assert.True(result.IsSuccess);
        Assert.Equal(entry.SourceTopic, result.SourceTopic);
        Assert.Single(publisher.Published);
        Assert.Equal(entry.SourceTopic, publisher.Published[0].Topic);
        var reprocessed = Assert.Single(dlq.Reprocessed);
        Assert.Equal(entry.EventId, reprocessed.EventId);
        Assert.Equal(Operator, reprocessed.OperatorIdentityId);
        var audit = Assert.Single(auditor.Calls);
        Assert.Equal(OperatorActionOutcomes.Accepted, audit.Outcome);
        Assert.Equal("operator-approved", audit.Rationale);
    }

    private static DeadLetterEntry NewEntry(Guid id, byte[]? payload = null) => new()
    {
        EventId = id,
        SourceTopic = "whyce.ctx.domain.events",
        EventType = "FooEvent",
        CorrelationId = id,
        CausationId = null,
        EnqueuedAt = new DateTimeOffset(2026, 4, 20, 12, 0, 0, TimeSpan.Zero),
        FailureCategory = RuntimeFailureCategory.DependencyUnavailable,
        LastError = "upstream timeout",
        AttemptCount = 3,
        Payload = payload ?? System.Text.Encoding.UTF8.GetBytes("{\"hello\":\"world\"}"),
        SchemaVersion = 1,
    };

    private static (DeadLetterRedriveService, InMemoryDlqStore, CapturingPublisher, CapturingRecorder) Build()
    {
        var dlq = new InMemoryDlqStore();
        var publisher = new CapturingPublisher();
        var auditor = new CapturingRecorder();
        var identity = new FakeCallerIdentity("tenant-a");
        var correlation = new FixedCorrelation(Guid.Parse("99999999-9999-9999-9999-999999999999"));
        var svc = new DeadLetterRedriveService(dlq, publisher, auditor, identity, correlation, new FixedClock());
        return (svc, dlq, publisher, auditor);
    }

    private sealed class InMemoryDlqStore : IDeadLetterStore
    {
        private readonly ConcurrentDictionary<Guid, DeadLetterEntry> _store = new();
        public List<(Guid EventId, string OperatorIdentityId)> Reprocessed { get; } = new();

        public void Record(DeadLetterEntry entry) => _store[entry.EventId] = entry;

        public Task RecordAsync(DeadLetterEntry entry, CancellationToken cancellationToken = default)
        {
            _store.TryAdd(entry.EventId, entry);
            return Task.CompletedTask;
        }

        public Task<DeadLetterEntry?> GetAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(eventId, out var e);
            return Task.FromResult<DeadLetterEntry?>(e);
        }

        public Task<IReadOnlyList<DeadLetterEntry>> ListAsync(string sourceTopic, DateTimeOffset? since = null, int limit = 100, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DeadLetterEntry>>(_store.Values.Where(e => e.SourceTopic == sourceTopic).ToList());

        public Task MarkReprocessedAsync(Guid eventId, string operatorIdentityId, DateTimeOffset reprocessedAt, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(eventId, out var existing))
            {
                _store[eventId] = existing with
                {
                    ReprocessedAt = reprocessedAt,
                    ReprocessedByIdentityId = operatorIdentityId,
                };
                Reprocessed.Add((eventId, operatorIdentityId));
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<DeadLetterEntry>> ListAllAsync(DateTimeOffset? since = null, int limit = 100, bool includeReprocessed = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DeadLetterEntry>>(_store.Values.Where(e => includeReprocessed || e.ReprocessedAt is null).ToList());
    }

    private sealed class CapturingPublisher : IDeadLetterRedrivePublisher
    {
        public List<(string Topic, byte[] Payload)> Published { get; } = new();
        public DeadLetterRedrivePublishException? ShouldThrow { get; set; }

        public Task PublishAsync(string topic, byte[] payload, Guid eventId, string eventType, int? schemaVersion, Guid correlationId, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow is not null) throw ShouldThrow;
            Published.Add((topic, payload));
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingRecorder : IOperatorActionRecorder
    {
        public List<OperatorActionEvent> Calls { get; } = new();

        public Task<OperatorActionEvent> RecordAsync(
            string actionType, Guid targetId, string targetResourceType,
            string operatorIdentityId, string tenantId, Guid correlationId,
            string outcome, string? rationale = null, string? failureReason = null,
            CancellationToken cancellationToken = default)
        {
            var evt = new OperatorActionEvent
            {
                EventId = Guid.NewGuid(),
                OperatorIdentityId = operatorIdentityId,
                ActionType = actionType,
                TargetId = targetId,
                TargetResourceType = targetResourceType,
                Rationale = rationale,
                Outcome = outcome,
                FailureReason = failureReason,
                CorrelationId = correlationId,
                TenantId = tenantId,
                OccurredAt = DateTimeOffset.UtcNow,
            };
            Calls.Add(evt);
            return Task.FromResult(evt);
        }
    }

    private sealed class FakeCallerIdentity : ICallerIdentityAccessor
    {
        private readonly string _tenant;
        public FakeCallerIdentity(string tenant) => _tenant = tenant;
        public string GetActorId() => "operator-fake";
        public string GetTenantId() => _tenant;
        public string[] GetRoles() => new[] { "admin" };
        public IReadOnlyDictionary<string, object> GetSubjectAttributes() => new Dictionary<string, object>();
    }

    private sealed class FixedCorrelation : IRequestCorrelationAccessor
    {
        public FixedCorrelation(Guid id) { Current = id; }
        public Guid Current { get; }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 4, 20, 14, 45, 0, TimeSpan.Zero);
    }
}
