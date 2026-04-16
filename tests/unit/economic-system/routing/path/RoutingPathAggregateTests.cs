using Whycespace.Domain.EconomicSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Routing.Path;

public sealed class RoutingPathAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero));

    private static RoutingPathAggregate NewDefined(string seed)
    {
        var pathId = new PathId(IdGen.Generate($"RoutingPathAggregateTests:{seed}:path"));
        return RoutingPathAggregate.DefinePath(pathId, PathType.Internal, "region=us-east", priority: 10);
    }

    [Fact]
    public void DefinePath_ThenActivate_ThenDisable_TransitionsThroughAllStates()
    {
        var aggregate = NewDefined("FullLifecycle");
        Assert.Equal(RoutingPathStatus.Defined, aggregate.Status);

        aggregate.Activate(Now);
        Assert.Equal(RoutingPathStatus.Active, aggregate.Status);

        aggregate.Disable(Now);
        Assert.Equal(RoutingPathStatus.Disabled, aggregate.Status);

        Assert.Equal(3, aggregate.DomainEvents.Count);
        Assert.IsType<RoutingPathDefinedEvent>(aggregate.DomainEvents[0]);
        Assert.IsType<RoutingPathActivatedEvent>(aggregate.DomainEvents[1]);
        Assert.IsType<RoutingPathDisabledEvent>(aggregate.DomainEvents[2]);
    }

    [Fact]
    public void Disable_FromDefined_Throws()
    {
        var aggregate = NewDefined("Disable_From_Defined");

        Assert.Throws<DomainException>(() => aggregate.Disable(Now));
    }

    [Fact]
    public void Activate_FromDisabled_Throws()
    {
        var aggregate = NewDefined("Activate_From_Disabled");
        aggregate.Activate(Now);
        aggregate.Disable(Now);
        Assert.Equal(RoutingPathStatus.Disabled, aggregate.Status);

        Assert.Throws<DomainException>(() => aggregate.Activate(Now));
    }
}
