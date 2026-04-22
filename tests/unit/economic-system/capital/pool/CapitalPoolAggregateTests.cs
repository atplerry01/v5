using Whycespace.Domain.EconomicSystem.Capital.Pool;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Capital.Pool;

public sealed class CapitalPoolAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static PoolId NewId(string seed) =>
        new(IdGen.Generate($"CapitalPoolAggregateTests:{seed}:pool"));

    [Fact]
    public void Create_RaisesPoolCreatedEvent()
    {
        var id = NewId("Create_Valid");

        var aggregate = CapitalPoolAggregate.Create(id, Usd, BaseTime);

        var evt = Assert.IsType<PoolCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.PoolId);
        Assert.Equal("USD", evt.Currency.Code);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");

        var aggregate = CapitalPoolAggregate.Create(id, Usd, BaseTime);

        Assert.Equal(id, aggregate.PoolId);
        Assert.Equal(0m, aggregate.TotalCapital.Value);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var p1 = CapitalPoolAggregate.Create(id, Usd, BaseTime);
        var p2 = CapitalPoolAggregate.Create(id, Usd, BaseTime);

        Assert.Equal(
            ((PoolCreatedEvent)p1.DomainEvents[0]).PoolId.Value,
            ((PoolCreatedEvent)p2.DomainEvents[0]).PoolId.Value);
    }

    [Fact]
    public void AggregateCapital_RaisesCapitalAggregatedEvent()
    {
        var id = NewId("Aggregate_Valid");
        var aggregate = CapitalPoolAggregate.Create(id, Usd, BaseTime);
        aggregate.ClearDomainEvents();

        var sourceId = IdGen.Generate("CapitalPoolAggregateTests:source");
        aggregate.AggregateCapital(sourceId, new Amount(1000m));

        var evt = Assert.IsType<CapitalAggregatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(1000m, evt.AggregatedAmount.Value);
        Assert.Equal(1000m, aggregate.TotalCapital.Value);
    }

    [Fact]
    public void ReduceCapital_RaisesCapitalReducedEvent()
    {
        var id = NewId("Reduce_Valid");
        var aggregate = CapitalPoolAggregate.Create(id, Usd, BaseTime);
        var sourceId = IdGen.Generate("CapitalPoolAggregateTests:source2");
        aggregate.AggregateCapital(sourceId, new Amount(2000m));
        aggregate.ClearDomainEvents();

        aggregate.ReduceCapital(sourceId, new Amount(500m));

        var evt = Assert.IsType<CapitalReducedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(500m, evt.ReducedAmount.Value);
        Assert.Equal(1500m, aggregate.TotalCapital.Value);
    }

    [Fact]
    public void ReduceCapital_ExceedsBalance_Throws()
    {
        var id = NewId("Reduce_Exceeds");
        var aggregate = CapitalPoolAggregate.Create(id, Usd, BaseTime);
        var sourceId = IdGen.Generate("CapitalPoolAggregateTests:source3");
        aggregate.AggregateCapital(sourceId, new Amount(100m));

        Assert.ThrowsAny<Exception>(() =>
            aggregate.ReduceCapital(sourceId, new Amount(200m)));
    }

    [Fact]
    public void AggregateCapital_ZeroAmount_Throws()
    {
        var id = NewId("Aggregate_Zero");
        var aggregate = CapitalPoolAggregate.Create(id, Usd, BaseTime);
        var sourceId = IdGen.Generate("CapitalPoolAggregateTests:source4");

        Assert.ThrowsAny<Exception>(() =>
            aggregate.AggregateCapital(sourceId, new Amount(0m)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesPoolState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new PoolCreatedEvent(id, Usd, BaseTime)
        };

        var aggregate = (CapitalPoolAggregate)Activator.CreateInstance(typeof(CapitalPoolAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.PoolId);
        Assert.Equal("USD", aggregate.Currency.Code);
        Assert.Equal(0m, aggregate.TotalCapital.Value);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void LoadFromHistory_AfterAggregation_RehydratesBalance()
    {
        var id = NewId("History_Aggregated");
        var sourceId = IdGen.Generate("CapitalPoolAggregateTests:history-source");

        var history = new object[]
        {
            new PoolCreatedEvent(id, Usd, BaseTime),
            new CapitalAggregatedEvent(id, sourceId, new Amount(5000m), new Amount(5000m))

        };

        var aggregate = (CapitalPoolAggregate)Activator.CreateInstance(typeof(CapitalPoolAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(5000m, aggregate.TotalCapital.Value);
        Assert.Empty(aggregate.DomainEvents);
    }
}
