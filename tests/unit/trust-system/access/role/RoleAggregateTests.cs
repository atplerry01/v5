using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Access.Role;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Access.Role;

public sealed class RoleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static RoleId NewId(string seed) =>
        new(IdGen.Generate($"RoleAggregateTests:{seed}:role"));

    private static RoleDescriptor DefaultDescriptor() =>
        new("content-moderator", "platform-wide");

    [Fact]
    public void Define_RaisesRoleDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = RoleAggregate.Define(id, descriptor, FixedTs);
        var evt = Assert.IsType<RoleDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RoleId);
        Assert.Equal(descriptor.RoleName, evt.Descriptor.RoleName);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = RoleAggregate.Define(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(RoleStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Activate_FromDefined_SetsStatusToActive()
    {
        var aggregate = RoleAggregate.Define(NewId("Activate"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Activate();
        Assert.IsType<RoleActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RoleStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Deprecate_FromActive_SetsStatusToDeprecated()
    {
        var aggregate = RoleAggregate.Define(NewId("Deprecate"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Deprecate();
        Assert.IsType<RoleDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RoleStatus.Deprecated, aggregate.Status);
    }

    [Fact]
    public void Activate_FromActive_Throws()
    {
        var aggregate = RoleAggregate.Define(NewId("Activate_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void Deprecate_FromDefined_Throws()
    {
        var aggregate = RoleAggregate.Define(NewId("Deprecate_Defined"), DefaultDescriptor(), FixedTs);
        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new RoleDefinedEvent(id, descriptor, FixedTs), new RoleActivatedEvent(id) };
        var aggregate = (RoleAggregate)Activator.CreateInstance(typeof(RoleAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.RoleId);
        Assert.Equal(RoleStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
