using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Ledger.Obligation;

public sealed class ObligationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp Later = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static ObligationId NewId(string seed) =>
        new(IdGen.Generate($"ObligationTests:{seed}:obligation"));

    private static CounterpartyRef NewCounterparty(string seed) =>
        new(IdGen.Generate($"ObligationTests:{seed}:counterparty"));

    [Fact]
    public void Create_RaisesObligationCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var counterparty = NewCounterparty("Create_Valid");

        var aggregate = ObligationAggregate.Create(id, counterparty, ObligationType.Payable, new Amount(1500m), Usd, BaseTime);

        var evt = Assert.IsType<ObligationCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ObligationId);
        Assert.Equal(1500m, evt.Amount.Value);
        Assert.Equal(ObligationType.Payable, evt.Type);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var counterparty = NewCounterparty("Create_State");

        var aggregate = ObligationAggregate.Create(id, counterparty, ObligationType.Receivable, new Amount(800m), Usd, BaseTime);

        Assert.Equal(id, aggregate.ObligationId);
        Assert.Equal(ObligationStatus.Pending, aggregate.Status);
        Assert.Equal(800m, aggregate.Amount.Value);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var counterparty = NewCounterparty("Stable");

        var o1 = ObligationAggregate.Create(id, counterparty, ObligationType.Payable, new Amount(100m), Usd, BaseTime);
        var o2 = ObligationAggregate.Create(id, counterparty, ObligationType.Payable, new Amount(100m), Usd, BaseTime);

        Assert.Equal(
            ((ObligationCreatedEvent)o1.DomainEvents[0]).ObligationId.Value,
            ((ObligationCreatedEvent)o2.DomainEvents[0]).ObligationId.Value);
    }

    [Fact]
    public void Create_ZeroAmount_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ObligationAggregate.Create(NewId("Zero"), NewCounterparty("Zero"), ObligationType.Payable, new Amount(0m), Usd, BaseTime));
    }

    [Fact]
    public void Fulfil_FromPending_RaisesFulfilledEvent()
    {
        var id = NewId("Fulfil");
        var aggregate = ObligationAggregate.Create(id, NewCounterparty("Fulfil"), ObligationType.Payable, new Amount(1000m), Usd, BaseTime);
        aggregate.ClearDomainEvents();
        var settlementId = IdGen.Generate("ObligationTests:Fulfil:settlement");

        aggregate.Fulfil(settlementId, Later);

        Assert.IsType<ObligationFulfilledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ObligationStatus.Fulfilled, aggregate.Status);
    }

    [Fact]
    public void Fulfil_AlreadyFulfilled_Throws()
    {
        var id = NewId("Fulfil_Again");
        var aggregate = ObligationAggregate.Create(id, NewCounterparty("Fulfil_Again"), ObligationType.Payable, new Amount(500m), Usd, BaseTime);
        aggregate.Fulfil(IdGen.Generate("ObligationTests:FA:s1"), BaseTime);

        Assert.ThrowsAny<Exception>(() => aggregate.Fulfil(IdGen.Generate("ObligationTests:FA:s2"), Later));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var counterparty = NewCounterparty("History");

        var history = new object[]
        {
            new ObligationCreatedEvent(id, counterparty.Value, ObligationType.Receivable, new Amount(2000m), Usd, BaseTime)
        };

        var aggregate = (ObligationAggregate)Activator.CreateInstance(typeof(ObligationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ObligationId);
        Assert.Equal(ObligationStatus.Pending, aggregate.Status);
        Assert.Equal(2000m, aggregate.Amount.Value);
        Assert.Empty(aggregate.DomainEvents);
    }
}
