using Whycespace.Domain.ControlSystem.AccessControl.Permission;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.AccessControl.Permission;

public sealed class PermissionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PermissionId NewId(string seed) =>
        new(Hex64($"PermissionAggregateTests:{seed}:permission"));

    [Fact]
    public void Define_RaisesPermissionDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = PermissionAggregate.Define(id, "read-docs", "document:*", ActionMask.Read);

        var evt = Assert.IsType<PermissionDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("read-docs", evt.Name);
        Assert.Equal(ActionMask.Read, evt.Actions);
    }

    [Fact]
    public void Define_SetsIsDeprecatedFalse()
    {
        var aggregate = PermissionAggregate.Define(NewId("State"), "p", "scope", ActionMask.Write);

        Assert.False(aggregate.IsDeprecated);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PermissionAggregate.Define(NewId("EmptyName"), "", "scope", ActionMask.Read));
    }

    [Fact]
    public void Define_WithNoActions_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PermissionAggregate.Define(NewId("NoActions"), "p", "scope", ActionMask.None));
    }

    [Fact]
    public void Define_WithEmptyScope_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PermissionAggregate.Define(NewId("EmptyScope"), "p", "", ActionMask.Read));
    }

    [Fact]
    public void Deprecate_RaisesPermissionDeprecatedEvent()
    {
        var aggregate = PermissionAggregate.Define(NewId("Deprecate"), "p", "scope", ActionMask.Admin);
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<PermissionDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsDeprecated);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = PermissionAggregate.Define(NewId("DoubleDeprecate"), "p", "scope", ActionMask.Read);
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new PermissionDefinedEvent(id, "p", "scope", ActionMask.All) };
        var aggregate = (PermissionAggregate)Activator.CreateInstance(typeof(PermissionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ActionMask.All, aggregate.Actions);
        Assert.Empty(aggregate.DomainEvents);
    }
}
