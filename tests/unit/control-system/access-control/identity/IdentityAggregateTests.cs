using Whycespace.Domain.ControlSystem.AccessControl.Identity;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.AccessControl.Identity;

public sealed class IdentityAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static IdentityId NewId(string seed) =>
        new(Hex64($"IdentityAggregateTests:{seed}:identity"));

    [Fact]
    public void Register_RaisesIdentityRegisteredEvent()
    {
        var id = NewId("Register");

        var aggregate = IdentityAggregate.Register(id, "service-account-1", IdentityKind.ServiceAccount);

        var evt = Assert.IsType<IdentityRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("service-account-1", evt.Name);
        Assert.Equal(IdentityKind.ServiceAccount, evt.Kind);
    }

    [Fact]
    public void Register_SetsStatusToActive()
    {
        var aggregate = IdentityAggregate.Register(NewId("Active"), "admin-bot", IdentityKind.AdminOperator);

        Assert.Equal(IdentityStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Register_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            IdentityAggregate.Register(NewId("EmptyName"), "", IdentityKind.ServiceAccount));
    }

    [Fact]
    public void Suspend_RaisesIdentitySuspendedEvent()
    {
        var aggregate = IdentityAggregate.Register(NewId("Suspend"), "service-1", IdentityKind.ServiceAccount);
        aggregate.ClearDomainEvents();

        aggregate.Suspend("policy violation");

        Assert.IsType<IdentitySuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IdentityStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Suspend_AlreadySuspended_Throws()
    {
        var aggregate = IdentityAggregate.Register(NewId("DoubleSuspend"), "service-1", IdentityKind.ServiceAccount);
        aggregate.Suspend("first reason");

        Assert.ThrowsAny<Exception>(() => aggregate.Suspend("second reason"));
    }

    [Fact]
    public void Deactivate_RaisesIdentityDeactivatedEvent()
    {
        var aggregate = IdentityAggregate.Register(NewId("Deactivate"), "service-1", IdentityKind.ServiceAccount);
        aggregate.ClearDomainEvents();

        aggregate.Deactivate();

        Assert.IsType<IdentityDeactivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IdentityStatus.Deactivated, aggregate.Status);
    }

    [Fact]
    public void Deactivate_AlreadyDeactivated_Throws()
    {
        var aggregate = IdentityAggregate.Register(NewId("DoubleDeactivate"), "service-1", IdentityKind.ServiceAccount);
        aggregate.Deactivate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deactivate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new IdentityRegisteredEvent(id, "service-1", IdentityKind.SystemAgent) };
        var aggregate = (IdentityAggregate)Activator.CreateInstance(typeof(IdentityAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(IdentityStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
