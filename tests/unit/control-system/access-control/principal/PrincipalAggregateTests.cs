using Whycespace.Domain.ControlSystem.AccessControl.Principal;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.AccessControl.Principal;

public sealed class PrincipalAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PrincipalId NewId(string seed) =>
        new(Hex64($"PrincipalAggregateTests:{seed}:principal"));

    [Fact]
    public void Register_RaisesPrincipalRegisteredEvent()
    {
        var id = NewId("Register");

        var aggregate = PrincipalAggregate.Register(id, "svc-agent", PrincipalKind.Service, "identity-1");

        var evt = Assert.IsType<PrincipalRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("svc-agent", evt.Name);
        Assert.Equal(PrincipalKind.Service, evt.Kind);
    }

    [Fact]
    public void Register_SetsStatusToActive()
    {
        var aggregate = PrincipalAggregate.Register(NewId("Active"), "p", PrincipalKind.Human, "id-1");

        Assert.Equal(PrincipalStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.RoleIds);
    }

    [Fact]
    public void Register_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PrincipalAggregate.Register(NewId("EmptyName"), "", PrincipalKind.Service, "id-1"));
    }

    [Fact]
    public void AssignRole_RaisesPrincipalRoleAssignedEvent()
    {
        var aggregate = PrincipalAggregate.Register(NewId("AssignRole"), "p", PrincipalKind.Service, "id-1");
        aggregate.ClearDomainEvents();

        aggregate.AssignRole("role-admin");

        var evt = Assert.IsType<PrincipalRoleAssignedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("role-admin", evt.RoleId);
        Assert.Contains("role-admin", aggregate.RoleIds);
    }

    [Fact]
    public void AssignRole_SameRoleTwice_DoesNotRaiseEvent()
    {
        var aggregate = PrincipalAggregate.Register(NewId("DuplicateRole"), "p", PrincipalKind.Service, "id-1");
        aggregate.AssignRole("role-1");
        aggregate.ClearDomainEvents();

        aggregate.AssignRole("role-1");

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AssignRole_WithEmptyRoleId_Throws()
    {
        var aggregate = PrincipalAggregate.Register(NewId("EmptyRole"), "p", PrincipalKind.Service, "id-1");

        Assert.ThrowsAny<Exception>(() => aggregate.AssignRole(""));
    }

    [Fact]
    public void Deactivate_RaisesPrincipalDeactivatedEvent()
    {
        var aggregate = PrincipalAggregate.Register(NewId("Deactivate"), "p", PrincipalKind.Service, "id-1");
        aggregate.ClearDomainEvents();

        aggregate.Deactivate();

        Assert.IsType<PrincipalDeactivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PrincipalStatus.Deactivated, aggregate.Status);
    }

    [Fact]
    public void Deactivate_AlreadyDeactivated_Throws()
    {
        var aggregate = PrincipalAggregate.Register(NewId("DoubleDeactivate"), "p", PrincipalKind.System, "id-1");
        aggregate.Deactivate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deactivate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new PrincipalRegisteredEvent(id, "p", PrincipalKind.Human, "id-1") };
        var aggregate = (PrincipalAggregate)Activator.CreateInstance(typeof(PrincipalAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PrincipalStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
