using Whycespace.Domain.ControlSystem.AccessControl.Role;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.AccessControl.Role;

public sealed class RoleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static RoleId NewId(string seed) =>
        new(Hex64($"RoleAggregateTests:{seed}:role"));

    [Fact]
    public void Define_RaisesRoleDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = RoleAggregate.Define(id, "admin-role", ["perm-1", "perm-2"]);

        var evt = Assert.IsType<RoleDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("admin-role", evt.Name);
        Assert.Equal(2, evt.PermissionIds.Count);
    }

    [Fact]
    public void Define_SetsIsDeprecatedFalse()
    {
        var aggregate = RoleAggregate.Define(NewId("State"), "role", []);

        Assert.False(aggregate.IsDeprecated);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            RoleAggregate.Define(NewId("EmptyName"), "", []));
    }

    [Fact]
    public void Define_WithParentRoleId_SetsParent()
    {
        var parentId = NewId("Parent");
        var aggregate = RoleAggregate.Define(NewId("Child"), "child-role", [], parentId.Value);

        Assert.Equal(parentId.Value, aggregate.ParentRoleId);
    }

    [Fact]
    public void AddPermission_RaisesRolePermissionAddedEvent()
    {
        var aggregate = RoleAggregate.Define(NewId("AddPerm"), "role", []);
        aggregate.ClearDomainEvents();

        aggregate.AddPermission("perm-new");

        var evt = Assert.IsType<RolePermissionAddedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("perm-new", evt.PermissionId);
        Assert.Contains("perm-new", aggregate.PermissionIds);
    }

    [Fact]
    public void AddPermission_SamePerm_DoesNotRaiseEvent()
    {
        var aggregate = RoleAggregate.Define(NewId("DupePerm"), "role", ["perm-1"]);
        aggregate.ClearDomainEvents();

        aggregate.AddPermission("perm-1");

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AddPermission_OnDeprecatedRole_Throws()
    {
        var aggregate = RoleAggregate.Define(NewId("DeprecatedAdd"), "role", []);
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.AddPermission("perm-1"));
    }

    [Fact]
    public void Deprecate_RaisesRoleDeprecatedEvent()
    {
        var aggregate = RoleAggregate.Define(NewId("Deprecate"), "role", []);
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<RoleDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsDeprecated);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = RoleAggregate.Define(NewId("DoubleDeprecate"), "role", []);
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new RoleDefinedEvent(id, "admin", new HashSet<string> { "perm-1" }, null)
        };
        var aggregate = (RoleAggregate)Activator.CreateInstance(typeof(RoleAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Contains("perm-1", aggregate.PermissionIds);
        Assert.Empty(aggregate.DomainEvents);
    }
}
