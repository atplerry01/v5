using Whycespace.Domain.TrustSystem.Access.Permission;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Access.Permission;

public sealed class PermissionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static PermissionId NewId(string seed) =>
        new(IdGen.Generate($"PermissionAggregateTests:{seed}:permission"));

    private static PermissionDescriptor DefaultDescriptor() =>
        new("document.read", "Document");

    [Fact]
    public void Define_RaisesPermissionDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = PermissionAggregate.Define(id, descriptor);

        var evt = Assert.IsType<PermissionDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.PermissionId);
        Assert.Equal(descriptor.PermissionName, evt.Descriptor.PermissionName);
        Assert.Equal(descriptor.ResourceType, evt.Descriptor.ResourceType);
    }

    [Fact]
    public void Define_SetsStatusToDefined()
    {
        var aggregate = PermissionAggregate.Define(NewId("State"), DefaultDescriptor());

        Assert.Equal(PermissionStatus.Defined, aggregate.Status);
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var descriptor = DefaultDescriptor();

        var p1 = PermissionAggregate.Define(id, descriptor);
        var p2 = PermissionAggregate.Define(id, descriptor);

        Assert.Equal(
            ((PermissionDefinedEvent)p1.DomainEvents[0]).PermissionId.Value,
            ((PermissionDefinedEvent)p2.DomainEvents[0]).PermissionId.Value);
    }

    [Fact]
    public void Activate_FromDefined_SetsStatusToActive()
    {
        var aggregate = PermissionAggregate.Define(NewId("Activate"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<PermissionActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PermissionStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Deprecate_FromActive_SetsStatusToDeprecated()
    {
        var aggregate = PermissionAggregate.Define(NewId("Deprecate"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<PermissionDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PermissionStatus.Deprecated, aggregate.Status);
    }

    [Fact]
    public void Deprecate_FromDefined_Throws()
    {
        var aggregate = PermissionAggregate.Define(NewId("Deprecate_Defined"), DefaultDescriptor());

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void Activate_AfterDeprecate_Throws()
    {
        var aggregate = PermissionAggregate.Define(NewId("Activate_Deprecated"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new PermissionDefinedEvent(id, descriptor)
        };

        var aggregate = (PermissionAggregate)Activator.CreateInstance(typeof(PermissionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor.PermissionName, aggregate.Descriptor.PermissionName);
        Assert.Equal(PermissionStatus.Defined, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
