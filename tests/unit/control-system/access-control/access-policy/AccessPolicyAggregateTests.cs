using Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.AccessControl.AccessPolicy;

public sealed class AccessPolicyAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AccessPolicyId NewId(string seed) =>
        new(Hex64($"AccessPolicyTests:{seed}:policy"));

    [Fact]
    public void Define_RaisesAccessPolicyDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = AccessPolicyAggregate.Define(id, "Read Policy", "document:*", ["role-admin"]);

        var evt = Assert.IsType<AccessPolicyDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("Read Policy", evt.Name);
        Assert.Equal("document:*", evt.Scope);
    }

    [Fact]
    public void Define_SetsStatusToDraft()
    {
        var aggregate = AccessPolicyAggregate.Define(NewId("Draft"), "P", "scope", ["role-1"]);

        Assert.Equal(AccessPolicyStatus.Draft, aggregate.Status);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AccessPolicyAggregate.Define(NewId("EmptyName"), "", "scope", ["role-1"]));
    }

    [Fact]
    public void Define_WithEmptyScope_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AccessPolicyAggregate.Define(NewId("EmptyScope"), "P", "", ["role-1"]));
    }

    [Fact]
    public void Activate_RaisesAccessPolicyActivatedEvent()
    {
        var aggregate = AccessPolicyAggregate.Define(NewId("Activate"), "P", "scope", ["role-1"]);
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<AccessPolicyActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AccessPolicyStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Activate_AlreadyActive_Throws()
    {
        var aggregate = AccessPolicyAggregate.Define(NewId("DoubleActivate"), "P", "scope", ["role-1"]);
        aggregate.Activate();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void Retire_RaisesAccessPolicyRetiredEvent()
    {
        var aggregate = AccessPolicyAggregate.Define(NewId("Retire"), "P", "scope", ["role-1"]);
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<AccessPolicyRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AccessPolicyStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new AccessPolicyDefinedEvent(id, "P", "scope", new HashSet<string> { "role-1" })
        };
        var aggregate = (AccessPolicyAggregate)Activator.CreateInstance(typeof(AccessPolicyAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AccessPolicyStatus.Draft, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
