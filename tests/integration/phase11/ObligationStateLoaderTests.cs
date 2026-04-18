using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Runtime.Middleware.Policy.Loaders;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase11;

/// <summary>
/// Phase 11 B1 — unit-level validation of <see cref="ObligationStateLoader"/>.
/// Exercises the loader against <see cref="InMemoryEventStore"/> pre-seeded
/// with domain events (mirroring the Postgres read-side which returns domain
/// types per <c>EVENT-STORE-HOLDS-MAPPED-PAYLOAD-01</c> rule 4).
///
/// Each test pins one invariant:
///   * factory-command on empty stream → null;
///   * unhandled command type → null even if events exist;
///   * empty aggregate id → null;
///   * created-only stream → <c>Status = "Pending"</c> with all fields
///     hydrated verbatim from <c>ObligationCreatedEvent</c>;
///   * fulfilled stream → <c>Status = "Fulfilled"</c>;
///   * cancelled stream → <c>Status = "Cancelled"</c>.
/// </summary>
public sealed class ObligationStateLoaderTests
{
    private static readonly Guid ObligationId = Guid.Parse("00000000-0000-0000-0000-000000000b01");
    private static readonly Guid Counterparty = Guid.Parse("00000000-0000-0000-0000-000000000b02");
    private static readonly Timestamp CreatedAt = new(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp LaterAt = new(new DateTimeOffset(2026, 4, 17, 13, 0, 0, TimeSpan.Zero));

    private static ObligationCreatedEvent CreatedEvent() =>
        new(
            new ObligationId(ObligationId),
            Counterparty,
            ObligationType.Payable,
            new Amount(250m),
            new Currency("USD"),
            CreatedAt);

    [Fact]
    public async Task FactoryCommand_EmptyStream_ReturnsNull()
    {
        var store = new InMemoryEventStore();
        var loader = new ObligationStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(CreateObligationCommand), ObligationId);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task UnhandledCommand_EvenWithEvents_ReturnsNull()
    {
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            ObligationId,
            RawEnvelopes.Wrap(ObligationId, CreatedEvent()),
            expectedVersion: -1);

        var loader = new ObligationStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            commandType: typeof(string),  // an unrelated type
            aggregateId: ObligationId);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task EmptyAggregateId_ReturnsNull()
    {
        var store = new InMemoryEventStore();
        var loader = new ObligationStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(FulfilObligationCommand), Guid.Empty);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task Created_ProducesPendingSnapshot_WithAllFields()
    {
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            ObligationId,
            RawEnvelopes.Wrap(ObligationId, CreatedEvent()),
            expectedVersion: -1);

        var loader = new ObligationStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(FulfilObligationCommand), ObligationId);

        var typed = Assert.IsType<ObligationStateSnapshot>(snapshot);
        Assert.Equal("Pending", typed.Status);
        Assert.Equal(250m, typed.Amount);
        Assert.Equal(Counterparty, typed.CounterpartyId);
        Assert.Equal("Payable", typed.Type);
        Assert.Equal("USD", typed.Currency);
    }

    [Fact]
    public async Task Fulfilled_ProducesFulfilledSnapshot()
    {
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            ObligationId,
            RawEnvelopes.Wrap(
                ObligationId,
                CreatedEvent(),
                new ObligationFulfilledEvent(
                    new ObligationId(ObligationId),
                    SettlementId: Guid.Parse("00000000-0000-0000-0000-000000000b03"),
                    LaterAt)),
            expectedVersion: -1);

        var loader = new ObligationStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(FulfilObligationCommand), ObligationId);

        var typed = Assert.IsType<ObligationStateSnapshot>(snapshot);
        Assert.Equal("Fulfilled", typed.Status);
    }

    [Fact]
    public async Task Cancelled_ProducesCancelledSnapshot()
    {
        var store = new InMemoryEventStore();
        await store.AppendEventsAsync(
            ObligationId,
            RawEnvelopes.Wrap(
                ObligationId,
                CreatedEvent(),
                new ObligationCancelledEvent(
                    new ObligationId(ObligationId),
                    Reason: "counterparty bankruptcy",
                    LaterAt)),
            expectedVersion: -1);

        var loader = new ObligationStateLoader(store);

        var snapshot = await loader.LoadSnapshotAsync(
            typeof(CancelObligationCommand), ObligationId);

        var typed = Assert.IsType<ObligationStateSnapshot>(snapshot);
        Assert.Equal("Cancelled", typed.Status);
    }

    [Fact]
    public async Task Handles_TypeDispatch_ExposedForCompositeRouter()
    {
        Assert.True(ObligationStateLoader.Handles(typeof(CreateObligationCommand)));
        Assert.True(ObligationStateLoader.Handles(typeof(FulfilObligationCommand)));
        Assert.True(ObligationStateLoader.Handles(typeof(CancelObligationCommand)));
        Assert.False(ObligationStateLoader.Handles(typeof(string)));
    }
}
