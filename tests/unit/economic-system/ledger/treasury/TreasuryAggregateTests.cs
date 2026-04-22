using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Ledger.Treasury;

public sealed class TreasuryAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static TreasuryId NewId(string seed) =>
        new(IdGen.Generate($"TreasuryAggregateTests:{seed}:treasury"));

    [Fact]
    public void Create_RaisesTreasuryCreatedEvent()
    {
        var id = NewId("Create_Valid");

        var aggregate = TreasuryAggregate.Create(id, Usd, BaseTime);

        var evt = Assert.IsType<TreasuryCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TreasuryId);
        Assert.Equal("USD", evt.Currency.Code);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");

        var aggregate = TreasuryAggregate.Create(id, Usd, BaseTime);

        Assert.Equal(id, aggregate.TreasuryId);
        Assert.Equal(0m, aggregate.Balance.Value);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var t1 = TreasuryAggregate.Create(id, Usd, BaseTime);
        var t2 = TreasuryAggregate.Create(id, Usd, BaseTime);

        Assert.Equal(
            ((TreasuryCreatedEvent)t1.DomainEvents[0]).TreasuryId.Value,
            ((TreasuryCreatedEvent)t2.DomainEvents[0]).TreasuryId.Value);
    }

    [Fact]
    public void ReleaseFunds_IncreasesBalance()
    {
        var id = NewId("Release");
        var aggregate = TreasuryAggregate.Create(id, Usd, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.ReleaseFunds(new Amount(5000m));

        var evt = Assert.IsType<TreasuryFundReleasedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(5000m, evt.NewBalance.Value);
        Assert.Equal(5000m, aggregate.Balance.Value);
    }

    [Fact]
    public void AllocateFunds_DecreasesBalance()
    {
        var id = NewId("Allocate");
        var aggregate = TreasuryAggregate.Create(id, Usd, BaseTime);
        aggregate.ReleaseFunds(new Amount(10000m));
        aggregate.ClearDomainEvents();

        aggregate.AllocateFunds(new Amount(3000m));

        var evt = Assert.IsType<TreasuryFundAllocatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(3000m, evt.AllocatedAmount.Value);
        Assert.Equal(7000m, aggregate.Balance.Value);
    }

    [Fact]
    public void AllocateFunds_ExceedsBalance_Throws()
    {
        var id = NewId("Allocate_Exceeds");
        var aggregate = TreasuryAggregate.Create(id, Usd, BaseTime);
        aggregate.ReleaseFunds(new Amount(100m));

        Assert.ThrowsAny<Exception>(() => aggregate.AllocateFunds(new Amount(200m)));
    }

    [Fact]
    public void AllocateFunds_ZeroAmount_Throws()
    {
        var id = NewId("Allocate_Zero");
        var aggregate = TreasuryAggregate.Create(id, Usd, BaseTime);
        aggregate.ReleaseFunds(new Amount(1000m));

        Assert.ThrowsAny<Exception>(() => aggregate.AllocateFunds(new Amount(0m)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesBalance()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new TreasuryCreatedEvent(id, Usd, BaseTime),
            new TreasuryFundReleasedEvent(id, new Amount(2500m), new Amount(2500m))
        };

        var aggregate = (TreasuryAggregate)Activator.CreateInstance(typeof(TreasuryAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.TreasuryId);
        Assert.Equal(2500m, aggregate.Balance.Value);
        Assert.Empty(aggregate.DomainEvents);
    }
}
