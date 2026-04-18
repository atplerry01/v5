using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Runtime.Middleware.Policy.Loaders;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase11;

/// <summary>
/// Phase 11 B2 — unit-level validation of <see cref="TreasuryStateLoader"/>.
/// Shape parity with <c>ObligationStateLoaderTests</c>.
/// </summary>
public sealed class TreasuryStateLoaderTests
{
    private static readonly Guid TreasuryId = Guid.Parse("00000000-0000-0000-0000-000000000c01");
    private static readonly Timestamp CreatedAt = new(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

    private static TreasuryCreatedEvent CreatedEvent() =>
        new(new TreasuryId(TreasuryId), new Currency("USD"), CreatedAt);

    [Fact]
    public async Task FactoryCommand_EmptyStream_ReturnsNull()
    {
        var store = new InMemoryEventStore();
        var loader = new TreasuryStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(CreateTreasuryCommand), TreasuryId);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task UnhandledCommand_ReturnsNull()
    {
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            TreasuryId,
            RawEnvelopes.Wrap(TreasuryId, CreatedEvent()),
            expectedVersion: -1);

        var loader = new TreasuryStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            commandType: typeof(string),
            aggregateId: TreasuryId);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task EmptyAggregateId_ReturnsNull()
    {
        var store = new InMemoryEventStore();
        var loader = new TreasuryStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(AllocateFundsCommand), Guid.Empty);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task Created_ProducesZeroBalance()
    {
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            TreasuryId,
            RawEnvelopes.Wrap(TreasuryId, CreatedEvent()),
            expectedVersion: -1);

        var loader = new TreasuryStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(AllocateFundsCommand), TreasuryId);

        var typed = Assert.IsType<TreasuryStateSnapshot>(snapshot);
        Assert.Equal(0m, typed.Balance);
        Assert.Equal("USD", typed.Currency);
    }

    [Fact]
    public async Task Released_ThenAllocated_ReflectsNetBalance()
    {
        // Released 500 → balance = 500; Allocated 200 → balance = 300.
        // The aggregate's own Apply reduces via TreasuryFund{Released,Allocated}Event.NewBalance.
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            TreasuryId,
            RawEnvelopes.Wrap(
                TreasuryId,
                CreatedEvent(),
                new TreasuryFundReleasedEvent(
                    new TreasuryId(TreasuryId),
                    ReleasedAmount: new Amount(500m),
                    NewBalance: new Amount(500m)),
                new TreasuryFundAllocatedEvent(
                    new TreasuryId(TreasuryId),
                    AllocatedAmount: new Amount(200m),
                    NewBalance: new Amount(300m))),
            expectedVersion: -1);

        var loader = new TreasuryStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(AllocateFundsCommand), TreasuryId);

        var typed = Assert.IsType<TreasuryStateSnapshot>(snapshot);
        Assert.Equal(300m, typed.Balance);
        Assert.Equal("USD", typed.Currency);
    }

    [Fact]
    public void Handles_TypeDispatch_ExposedForCompositeRouter()
    {
        Assert.True(TreasuryStateLoader.Handles(typeof(CreateTreasuryCommand)));
        Assert.True(TreasuryStateLoader.Handles(typeof(AllocateFundsCommand)));
        Assert.True(TreasuryStateLoader.Handles(typeof(ReleaseFundsCommand)));
        Assert.False(TreasuryStateLoader.Handles(typeof(string)));
    }
}
