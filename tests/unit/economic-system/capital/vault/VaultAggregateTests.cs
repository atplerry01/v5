using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Capital.Vault;

public sealed class VaultAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");
    private static readonly Currency Eur = new("EUR");

    private static VaultId NewVaultId(string seed) =>
        new(IdGen.Generate($"VaultAggregateTests:{seed}:vault"));

    private static OwnerId NewOwnerId(string seed) =>
        new(IdGen.Generate($"VaultAggregateTests:{seed}:owner"));

    private static SliceId NewSliceId(string seed) =>
        new(IdGen.Generate($"VaultAggregateTests:{seed}:slice"));

    [Fact]
    public void Create_RaisesVaultCreatedEvent()
    {
        var id = NewVaultId("Create_Valid");
        var owner = NewOwnerId("Create_Valid");

        var aggregate = VaultAggregate.Create(id, owner, Usd, BaseTime);

        var evt = Assert.IsType<VaultCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.VaultId);
        Assert.Equal("USD", evt.Currency.Code);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewVaultId("Create_State");
        var owner = NewOwnerId("Create_State");

        var aggregate = VaultAggregate.Create(id, owner, Usd, BaseTime);

        Assert.Equal(id, aggregate.VaultId);
        Assert.Equal(0m, aggregate.TotalStored.Value);
        Assert.Empty(aggregate.Slices);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewVaultId("Stable");
        var owner = NewOwnerId("Stable");

        var v1 = VaultAggregate.Create(id, owner, Usd, BaseTime);
        var v2 = VaultAggregate.Create(id, owner, Usd, BaseTime);

        Assert.Equal(
            ((VaultCreatedEvent)v1.DomainEvents[0]).VaultId.Value,
            ((VaultCreatedEvent)v2.DomainEvents[0]).VaultId.Value);
    }

    [Fact]
    public void AddSlice_RaisesVaultSliceCreatedEvent()
    {
        var id = NewVaultId("AddSlice");
        var aggregate = VaultAggregate.Create(id, NewOwnerId("AddSlice"), Usd, BaseTime);
        var sliceId = NewSliceId("AddSlice");
        aggregate.ClearDomainEvents();

        aggregate.AddSlice(sliceId, new Amount(5000m), Usd);

        var evt = Assert.IsType<VaultSliceCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(sliceId, evt.SliceId);
        Assert.Equal(5000m, evt.TotalCapacity.Value);
        Assert.Single(aggregate.Slices);
        Assert.Equal(5000m, aggregate.TotalStored.Value);
    }

    [Fact]
    public void AddSlice_WrongCurrency_Throws()
    {
        var id = NewVaultId("AddSlice_Currency");
        var aggregate = VaultAggregate.Create(id, NewOwnerId("AddSlice_Currency"), Usd, BaseTime);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.AddSlice(NewSliceId("Currency"), new Amount(1000m), Eur));
    }

    [Fact]
    public void AddSlice_ZeroCapacity_Throws()
    {
        var id = NewVaultId("AddSlice_Zero");
        var aggregate = VaultAggregate.Create(id, NewOwnerId("AddSlice_Zero"), Usd, BaseTime);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.AddSlice(NewSliceId("Zero"), new Amount(0m), Usd));
    }

    [Fact]
    public void DepositToSlice_IncreasesVaultTotal()
    {
        var id = NewVaultId("Deposit");
        var aggregate = VaultAggregate.Create(id, NewOwnerId("Deposit"), Usd, BaseTime);
        var sliceId = NewSliceId("Deposit");
        aggregate.AddSlice(sliceId, new Amount(3000m), Usd);
        aggregate.ClearDomainEvents();

        aggregate.DepositToSlice(sliceId, new Amount(1000m));

        var evt = Assert.IsType<CapitalDepositedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(1000m, evt.DepositedAmount.Value);
    }

    [Fact]
    public void AllocateFromSlice_DecreasesAvailable()
    {
        var id = NewVaultId("Allocate");
        var aggregate = VaultAggregate.Create(id, NewOwnerId("Allocate"), Usd, BaseTime);
        var sliceId = NewSliceId("Allocate");
        aggregate.AddSlice(sliceId, new Amount(5000m), Usd);
        aggregate.ClearDomainEvents();

        aggregate.AllocateFromSlice(sliceId, new Amount(2000m));

        var slice = Assert.Single(aggregate.Slices);
        Assert.Equal(3000m, slice.AvailableAmount.Value);
        Assert.Equal(2000m, slice.UsedAmount.Value);
    }

    [Fact]
    public void AllocateFromSlice_ExceedsAvailable_Throws()
    {
        var id = NewVaultId("Allocate_Exceeds");
        var aggregate = VaultAggregate.Create(id, NewOwnerId("Allocate_Exceeds"), Usd, BaseTime);
        var sliceId = NewSliceId("Allocate_Exceeds");
        aggregate.AddSlice(sliceId, new Amount(1000m), Usd);

        Assert.ThrowsAny<Exception>(() => aggregate.AllocateFromSlice(sliceId, new Amount(2000m)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesVaultState()
    {
        var id = NewVaultId("History");
        var owner = NewOwnerId("History");

        var history = new object[]
        {
            new VaultCreatedEvent(id, owner.Value, Usd, BaseTime)
        };

        var aggregate = (VaultAggregate)Activator.CreateInstance(typeof(VaultAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.VaultId);
        Assert.Equal(0m, aggregate.TotalStored.Value);
        Assert.Empty(aggregate.Slices);
        Assert.Empty(aggregate.DomainEvents);
    }
}
