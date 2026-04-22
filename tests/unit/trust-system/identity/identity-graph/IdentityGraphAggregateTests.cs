using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.IdentityGraph;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityGraphAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static IdentityGraphId NewId(string seed) =>
        new(IdGen.Generate($"IdentityGraphAggregateTests:{seed}:identity-graph"));

    private static IdentityGraphDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("IdentityGraphAggregateTests:primary-ref"), "enterprise-graph");

    [Fact]
    public void Initialize_RaisesIdentityGraphInitializedEvent()
    {
        var id = NewId("Initialize_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = IdentityGraphAggregate.Initialize(id, descriptor, FixedTs);
        var evt = Assert.IsType<IdentityGraphInitializedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.IdentityGraphId);
        Assert.Equal(descriptor.GraphContext, evt.Descriptor.GraphContext);
    }

    [Fact]
    public void Initialize_SetsStatusToActive()
    {
        var aggregate = IdentityGraphAggregate.Initialize(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(IdentityGraphStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Archive_FromActive_SetsStatusToArchived()
    {
        var aggregate = IdentityGraphAggregate.Initialize(NewId("Archive"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Archive();
        Assert.IsType<IdentityGraphArchivedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IdentityGraphStatus.Archived, aggregate.Status);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_Throws()
    {
        var aggregate = IdentityGraphAggregate.Initialize(NewId("Archive_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Archive();
        Assert.ThrowsAny<Exception>(() => aggregate.Archive());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new IdentityGraphInitializedEvent(id, descriptor, FixedTs), new IdentityGraphArchivedEvent(id) };
        var aggregate = (IdentityGraphAggregate)Activator.CreateInstance(typeof(IdentityGraphAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.IdentityGraphId);
        Assert.Equal(IdentityGraphStatus.Archived, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
