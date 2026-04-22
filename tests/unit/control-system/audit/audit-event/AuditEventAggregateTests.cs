using Whycespace.Domain.ControlSystem.Audit.AuditEvent;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Audit.AuditEvent;

public sealed class AuditEventAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AuditEventId NewId(string seed) =>
        new(Hex64($"AuditEventTests:{seed}:event"));

    [Fact]
    public void Capture_RaisesAuditEventCapturedEvent()
    {
        var id = NewId("Capture");

        var aggregate = AuditEventAggregate.Capture(id, "actor-1", "create:document", AuditEventKind.AccessDecision, "corr-1", BaseTime);

        var evt = Assert.IsType<AuditEventCapturedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("actor-1", evt.ActorId);
        Assert.Equal(AuditEventKind.AccessDecision, evt.Kind);
    }

    [Fact]
    public void Capture_SetsIsSealedFalse()
    {
        var aggregate = AuditEventAggregate.Capture(NewId("State"), "actor-1", "action", AuditEventKind.PolicyEvaluation, "corr", BaseTime);

        Assert.False(aggregate.IsSealed);
        Assert.Null(aggregate.IntegrityHash);
    }

    [Fact]
    public void Capture_WithEmptyActorId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditEventAggregate.Capture(NewId("EmptyActor"), "", "action", AuditEventKind.SystemAction, "corr", BaseTime));
    }

    [Fact]
    public void Capture_WithEmptyAction_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditEventAggregate.Capture(NewId("EmptyAction"), "actor-1", "", AuditEventKind.SystemAction, "corr", BaseTime));
    }

    [Fact]
    public void Seal_RaisesAuditEventSealedEvent()
    {
        var aggregate = AuditEventAggregate.Capture(NewId("Seal"), "actor-1", "action", AuditEventKind.IdentityAction, "corr", BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Seal("sha256-hash-abc");

        var evt = Assert.IsType<AuditEventSealedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("sha256-hash-abc", evt.IntegrityHash);
        Assert.True(aggregate.IsSealed);
    }

    [Fact]
    public void Seal_AlreadySealed_Throws()
    {
        var aggregate = AuditEventAggregate.Capture(NewId("DoubleSeal"), "actor-1", "action", AuditEventKind.ConfigurationChange, "corr", BaseTime);
        aggregate.Seal("hash-1");

        Assert.ThrowsAny<Exception>(() => aggregate.Seal("hash-2"));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new AuditEventCapturedEvent(id, "actor-1", "action", AuditEventKind.SecurityIncident, "corr", BaseTime)
        };
        var aggregate = (AuditEventAggregate)Activator.CreateInstance(typeof(AuditEventAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.False(aggregate.IsSealed);
        Assert.Empty(aggregate.DomainEvents);
    }
}
