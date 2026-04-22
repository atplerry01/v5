using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Capital.Account;

public sealed class CapitalAccountAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");
    private static readonly Currency Eur = new("EUR");

    private static AccountId NewAccountId(string seed) =>
        new(IdGen.Generate($"CapitalAccountTests:{seed}:account"));

    private static OwnerId NewOwnerId(string seed) =>
        new(IdGen.Generate($"CapitalAccountTests:{seed}:owner"));

    private static CapitalAccountAggregate OpenAccount(string seed)
    {
        var aggregate = (CapitalAccountAggregate)Activator.CreateInstance(typeof(CapitalAccountAggregate), nonPublic: true)!;
        aggregate.Open(NewAccountId(seed), NewOwnerId(seed), Usd, BaseTime);
        return aggregate;
    }

    [Fact]
    public void Open_RaisesCapitalAccountOpenedEvent()
    {
        var accountId = NewAccountId("Open_Valid");
        var ownerId = NewOwnerId("Open_Valid");
        var aggregate = (CapitalAccountAggregate)Activator.CreateInstance(typeof(CapitalAccountAggregate), nonPublic: true)!;

        aggregate.Open(accountId, ownerId, Usd, BaseTime);

        var evt = Assert.IsType<CapitalAccountOpenedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(accountId, evt.AccountId);
        Assert.Equal("USD", evt.Currency.Code);
    }

    [Fact]
    public void Open_SetsActiveStatusAndZeroBalances()
    {
        var aggregate = OpenAccount("State");

        Assert.Equal(CapitalAccountStatus.Active, aggregate.Status);
        Assert.Equal(0m, aggregate.TotalBalance.Value);
        Assert.Equal(0m, aggregate.AvailableBalance.Value);
        Assert.Equal(0m, aggregate.ReservedBalance.Value);
    }

    [Fact]
    public void Fund_IncreasesTotalAndAvailableBalance()
    {
        var aggregate = OpenAccount("Fund");
        aggregate.ClearDomainEvents();

        aggregate.Fund(new Amount(1000m), Usd);

        var evt = Assert.IsType<CapitalFundedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(1000m, evt.NewTotalBalance.Value);
        Assert.Equal(1000m, aggregate.TotalBalance.Value);
        Assert.Equal(1000m, aggregate.AvailableBalance.Value);
    }

    [Fact]
    public void Reserve_MovesAmountFromAvailableToReserved()
    {
        var aggregate = OpenAccount("Reserve");
        aggregate.Fund(new Amount(1000m), Usd);
        aggregate.ClearDomainEvents();

        aggregate.Reserve(new Amount(300m), Usd);

        Assert.Equal(700m, aggregate.AvailableBalance.Value);
        Assert.Equal(300m, aggregate.ReservedBalance.Value);
        Assert.Equal(1000m, aggregate.TotalBalance.Value);
    }

    [Fact]
    public void Reserve_ExceedsAvailable_Throws()
    {
        var aggregate = OpenAccount("Reserve_Exceeds");
        aggregate.Fund(new Amount(100m), Usd);

        Assert.ThrowsAny<Exception>(() => aggregate.Reserve(new Amount(200m), Usd));
    }

    [Fact]
    public void Freeze_SetsStatusToFrozen()
    {
        var aggregate = OpenAccount("Freeze");
        aggregate.ClearDomainEvents();

        aggregate.Freeze("Suspicious activity.");

        Assert.IsType<CapitalAccountFrozenEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(CapitalAccountStatus.Frozen, aggregate.Status);
    }

    [Fact]
    public void Fund_WhenFrozen_Throws()
    {
        var aggregate = OpenAccount("Fund_Frozen");
        aggregate.Freeze("Locked.");

        Assert.ThrowsAny<Exception>(() => aggregate.Fund(new Amount(100m), Usd));
    }

    [Fact]
    public void Close_WithZeroBalance_SetsStatusToClosed()
    {
        var aggregate = OpenAccount("Close");
        aggregate.ClearDomainEvents();

        aggregate.Close(BaseTime);

        Assert.IsType<CapitalAccountClosedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(CapitalAccountStatus.Closed, aggregate.Status);
    }

    [Fact]
    public void Close_WithOutstandingBalance_Throws()
    {
        var aggregate = OpenAccount("Close_Balance");
        aggregate.Fund(new Amount(500m), Usd);

        Assert.ThrowsAny<Exception>(() => aggregate.Close(BaseTime));
    }

    [Fact]
    public void Fund_WrongCurrency_Throws()
    {
        var aggregate = OpenAccount("Fund_Currency");

        Assert.ThrowsAny<Exception>(() => aggregate.Fund(new Amount(100m), Eur));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var accountId = NewAccountId("History");
        var ownerId = NewOwnerId("History");

        var history = new object[]
        {
            new CapitalAccountOpenedEvent(accountId, ownerId, Usd, BaseTime),
            new CapitalFundedEvent(accountId, new Amount(2000m), new Amount(2000m), new Amount(2000m)) // fundedAmount, newTotal, newAvailable
        };

        var aggregate = (CapitalAccountAggregate)Activator.CreateInstance(typeof(CapitalAccountAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(accountId, aggregate.AccountId);
        Assert.Equal(CapitalAccountStatus.Active, aggregate.Status);
        Assert.Equal(2000m, aggregate.TotalBalance.Value);
        Assert.Empty(aggregate.DomainEvents);
    }
}
