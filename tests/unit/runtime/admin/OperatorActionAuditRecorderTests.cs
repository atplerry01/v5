using System.Collections.Concurrent;
using Whycespace.Runtime.ControlPlane.Admin;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime.Admin;

/// <summary>
/// R4.B / R-ADMIN-AUDIT-EMISSION-01 — operator-action recorder invariants:
/// deterministic event id, canonical audit routing classification / context
/// / domain, metadata carries the evidence set, every required field is
/// validated before emission.
/// </summary>
public sealed class OperatorActionAuditRecorderTests
{
    [Fact]
    public async Task Record_emits_event_on_audit_stream_with_canonical_routing()
    {
        var fabric = new RecordingFabric();
        var recorder = new OperatorActionAuditRecorder(fabric, new StaticIdGenerator(), new FixedClock());

        var correlationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var targetId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var evt = await recorder.RecordAsync(
            actionType: "outbound-effect.reconcile",
            targetId: targetId,
            targetResourceType: "outbound-effect",
            operatorIdentityId: "operator-alice",
            tenantId: "tenant-a",
            correlationId: correlationId,
            outcome: OperatorActionOutcomes.Accepted,
            rationale: "customer-requested settlement override");

        Assert.Single(fabric.AuditEmissions);
        var emission = fabric.AuditEmissions[0];
        Assert.Equal(AdminScope.AuditClassification, emission.Audit.Classification);
        Assert.Equal(AdminScope.AuditContext, emission.Audit.Context);
        Assert.Equal(AdminScope.AuditDomain, emission.Audit.Domain);
        Assert.Single(emission.Audit.Events);
        Assert.Same(evt, emission.Audit.Events[0]);
        Assert.Equal(evt.EventId, emission.Audit.AggregateId);
        Assert.Equal(evt.EventId, emission.Context.CommandId);
        Assert.Equal("operator-alice", emission.Context.ActorId);
        Assert.Equal("tenant-a", emission.Context.TenantId);
        Assert.Equal(correlationId, emission.Context.CorrelationId);
        Assert.False(emission.Context.IsSystem);

        Assert.Equal("outbound-effect.reconcile", emission.Audit.Metadata["ActionType"]);
        Assert.Equal(targetId.ToString(), emission.Audit.Metadata["TargetId"]);
        Assert.Equal("accepted", emission.Audit.Metadata["Outcome"]);
    }

    [Fact]
    public async Task Record_derives_event_id_from_correlation_action_target()
    {
        var fabric = new RecordingFabric();
        var recorder = new OperatorActionAuditRecorder(fabric, new StaticIdGenerator(), new FixedClock());

        var a = await recorder.RecordAsync(
            "dlq.redrive", Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "dead-letter-entry",
            "op", "tenant", Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            OperatorActionOutcomes.Accepted);
        var b = await recorder.RecordAsync(
            "dlq.redrive", Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "dead-letter-entry",
            "op", "tenant", Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            OperatorActionOutcomes.Accepted);
        var c = await recorder.RecordAsync(
            "dlq.redrive", Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "dead-letter-entry",
            "op", "tenant", Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            OperatorActionOutcomes.Accepted);

        Assert.Equal(a.EventId, b.EventId);
        Assert.NotEqual(a.EventId, c.EventId);
    }

    [Theory]
    [InlineData("", "op", "tenant", "accepted")]
    [InlineData("action", "", "tenant", "accepted")]
    [InlineData("action", "op", "", "accepted")]
    [InlineData("action", "op", "tenant", "")]
    public async Task Record_validates_required_fields(string actionType, string operatorId, string tenantId, string outcome)
    {
        var recorder = new OperatorActionAuditRecorder(new RecordingFabric(), new StaticIdGenerator(), new FixedClock());

        await Assert.ThrowsAsync<ArgumentException>(() => recorder.RecordAsync(
            actionType, Guid.NewGuid(), "resource",
            operatorId, tenantId, Guid.NewGuid(),
            outcome));
    }

    private sealed record Emission(AuditEmission Audit, CommandContext Context);

    private sealed class RecordingFabric : IEventFabric
    {
        public List<Emission> AuditEmissions { get; } = new();

        public Task ProcessAsync(IReadOnlyList<object> domainEvents, CommandContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ProcessAuditAsync(AuditEmission audit, CommandContext context, CancellationToken cancellationToken = default)
        {
            AuditEmissions.Add(new Emission(audit, context));
            return Task.CompletedTask;
        }
    }

    private sealed class StaticIdGenerator : IIdGenerator
    {
        // Deterministic Guid per seed string using a stable hash.
        private readonly ConcurrentDictionary<string, Guid> _cache = new();

        public Guid Generate(string seed) =>
            _cache.GetOrAdd(seed, s =>
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
                var guidBytes = new byte[16];
                Array.Copy(bytes, guidBytes, 16);
                return new Guid(guidBytes);
            });
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 4, 20, 14, 30, 0, TimeSpan.Zero);
    }
}
