using Whycespace.Domain.ControlSystem.Audit.AuditLog;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Audit.AuditLog;

public sealed class AuditLogAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AuditLogId NewId(string seed) =>
        new(Hex64($"AuditLogTests:{seed}:log"));

    [Fact]
    public void Record_RaisesAuditEntryRecordedEvent()
    {
        var id = NewId("Record");

        var aggregate = AuditLogAggregate.Record(id, "actor-1", "create", "document/123", AuditEntryClassification.Command, BaseTime);

        var evt = Assert.IsType<AuditEntryRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("actor-1", evt.ActorId);
        Assert.Equal(AuditEntryClassification.Command, evt.Classification);
        Assert.Null(evt.DecisionId);
    }

    [Fact]
    public void Record_WithDecisionId_SetsDecisionId()
    {
        var aggregate = AuditLogAggregate.Record(
            NewId("WithDecision"), "actor-1", "action", "resource", AuditEntryClassification.Policy, BaseTime, "decision-abc");

        Assert.Equal("decision-abc", aggregate.DecisionId);
    }

    [Fact]
    public void Record_WithEmptyActorId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditLogAggregate.Record(NewId("EmptyActor"), "", "action", "resource", AuditEntryClassification.Access, BaseTime));
    }

    [Fact]
    public void Record_WithEmptyAction_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditLogAggregate.Record(NewId("EmptyAction"), "actor-1", "", "resource", AuditEntryClassification.State, BaseTime));
    }

    [Fact]
    public void Record_WithEmptyResource_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditLogAggregate.Record(NewId("EmptyResource"), "actor-1", "action", "", AuditEntryClassification.Command, BaseTime));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new AuditEntryRecordedEvent(id, "actor-1", "create", "resource", AuditEntryClassification.Command, BaseTime, null)
        };
        var aggregate = (AuditLogAggregate)Activator.CreateInstance(typeof(AuditLogAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("actor-1", aggregate.ActorId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
