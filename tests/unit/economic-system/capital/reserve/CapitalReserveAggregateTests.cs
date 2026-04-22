using Whycespace.Domain.EconomicSystem.Capital.Reserve;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Capital.Reserve;

public sealed class CapitalReserveAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 22, 17, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static ReserveId NewId(string seed) =>
        new(IdGen.Generate($"ReserveTests:{seed}:reserve"));

    private static AccountId NewAccountId(string seed) =>
        new(IdGen.Generate($"ReserveTests:{seed}:account"));

    [Fact]
    public void Create_RaisesReserveCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var accountId = NewAccountId("Create_Valid");

        var aggregate = ReserveAggregate.Create(id, accountId, new Amount(2000m), Usd, T0, T2);

        var evt = Assert.IsType<ReserveCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ReserveId);
        Assert.Equal(2000m, evt.ReservedAmount.Value);
    }

    [Fact]
    public void Create_SetsStatusToActive()
    {
        var id = NewId("Create_State");
        var aggregate = ReserveAggregate.Create(id, NewAccountId("Create_State"), new Amount(1000m), Usd, T0, T2);

        Assert.Equal(ReserveStatus.Active, aggregate.Status);
        Assert.Equal(1000m, aggregate.Amount.Value);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var accountId = NewAccountId("Stable");

        var r1 = ReserveAggregate.Create(id, accountId, new Amount(500m), Usd, T0, T2);
        var r2 = ReserveAggregate.Create(id, accountId, new Amount(500m), Usd, T0, T2);

        Assert.Equal(
            ((ReserveCreatedEvent)r1.DomainEvents[0]).ReserveId.Value,
            ((ReserveCreatedEvent)r2.DomainEvents[0]).ReserveId.Value);
    }

    [Fact]
    public void Create_ZeroAmount_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ReserveAggregate.Create(NewId("Zero"), NewAccountId("Zero"), new Amount(0m), Usd, T0, T2));
    }

    [Fact]
    public void Create_ExpiryBeforeReservedAt_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ReserveAggregate.Create(NewId("Expiry"), NewAccountId("Expiry"), new Amount(100m), Usd, T2, T0));
    }

    [Fact]
    public void Release_FromActive_SetsStatusToReleased()
    {
        var aggregate = ReserveAggregate.Create(NewId("Release"), NewAccountId("Release"), new Amount(3000m), Usd, T0, T2);
        aggregate.ClearDomainEvents();

        aggregate.Release(T1);

        Assert.Equal(ReserveStatus.Released, aggregate.Status);
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var accountId = NewAccountId("History");

        var history = new object[]
        {
            new ReserveCreatedEvent(id, accountId.Value, new Amount(4000m), Usd, T0, T2)
        };

        var aggregate = (ReserveAggregate)Activator.CreateInstance(typeof(ReserveAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ReserveId);
        Assert.Equal(ReserveStatus.Active, aggregate.Status);
        Assert.Equal(4000m, aggregate.Amount.Value);
        Assert.Empty(aggregate.DomainEvents);
    }
}
