using Whycespace.Domain.EconomicSystem.Exchange.Rate;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using DomainExchangeRate = Whycespace.Domain.EconomicSystem.Exchange.Rate.ExchangeRate;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Exchange.Rate;

public sealed class ExchangeRateAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");
    private static readonly Currency Eur = new("EUR");

    private static RateId NewId(string seed) =>
        new(IdGen.Generate($"ExchangeRateTests:{seed}:rate"));

    [Fact]
    public void DefineRate_RaisesExchangeRateDefinedEvent()
    {
        var id = NewId("Define_Valid");

        var aggregate = ExchangeRateAggregate.DefineRate(id, Usd, Eur, new DomainExchangeRate(1.08m), T0, 1);

        var evt = Assert.IsType<ExchangeRateDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RateId);
        Assert.Equal(1.08m, evt.RateValue);
        Assert.Equal(1, evt.Version);
    }

    [Fact]
    public void DefineRate_SetsStatusToDefined()
    {
        var aggregate = ExchangeRateAggregate.DefineRate(NewId("State"), Usd, Eur, new DomainExchangeRate(1.08m), T0, 1);

        Assert.Equal(ExchangeRateStatus.Defined, aggregate.Status);
        Assert.Equal(1.08m, aggregate.RateValue.Value);
    }

    [Fact]
    public void DefineRate_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");

        var r1 = ExchangeRateAggregate.DefineRate(id, Usd, Eur, new DomainExchangeRate(1.10m), T0, 1);
        var r2 = ExchangeRateAggregate.DefineRate(id, Usd, Eur, new DomainExchangeRate(1.10m), T0, 1);

        Assert.Equal(
            ((ExchangeRateDefinedEvent)r1.DomainEvents[0]).RateId.Value,
            ((ExchangeRateDefinedEvent)r2.DomainEvents[0]).RateId.Value);
    }

    [Fact]
    public void DefineRate_ZeroRate_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ExchangeRateAggregate.DefineRate(NewId("Zero"), Usd, Eur, 0m, T0, 1));
    }

    [Fact]
    public void Activate_FromDefined_SetsStatusToActive()
    {
        var aggregate = ExchangeRateAggregate.DefineRate(NewId("Activate"), Usd, Eur, new DomainExchangeRate(1.05m), T0, 1);
        aggregate.ClearDomainEvents();

        aggregate.Activate(T1);

        Assert.IsType<ExchangeRateActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ExchangeRateStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Expire_FromActive_SetsStatusToExpired()
    {
        var aggregate = ExchangeRateAggregate.DefineRate(NewId("Expire"), Usd, Eur, new DomainExchangeRate(1.05m), T0, 1);
        aggregate.Activate(T0);
        aggregate.ClearDomainEvents();

        aggregate.Expire(T1);

        Assert.IsType<ExchangeRateExpiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ExchangeRateStatus.Expired, aggregate.Status);
    }

    [Fact]
    public void Activate_AfterExpire_Throws()
    {
        var aggregate = ExchangeRateAggregate.DefineRate(NewId("Activate_Expired"), Usd, Eur, new DomainExchangeRate(1.05m), T0, 1);
        aggregate.Activate(T0);
        aggregate.Expire(T1);

        Assert.ThrowsAny<Exception>(() => aggregate.Activate(T1));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new ExchangeRateDefinedEvent(id, Usd, Eur, 1.08m, T0, 2)
        };

        var aggregate = (ExchangeRateAggregate)Activator.CreateInstance(typeof(ExchangeRateAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.RateId);
        Assert.Equal(1.08m, aggregate.RateValue.Value);
        Assert.Equal(ExchangeRateStatus.Defined, aggregate.Status);
        Assert.Equal(2, aggregate.Version);
        Assert.Empty(aggregate.DomainEvents);
    }
}
