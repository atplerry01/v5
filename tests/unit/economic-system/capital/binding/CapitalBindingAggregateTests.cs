using Whycespace.Domain.EconomicSystem.Capital.Binding;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Capital.Binding;

public sealed class CapitalBindingAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));

    private static BindingId NewId(string seed) =>
        new(IdGen.Generate($"BindingTests:{seed}:binding"));

    private static AccountId NewAccountId(string seed) =>
        new(IdGen.Generate($"BindingTests:{seed}:account"));

    private static OwnerId NewOwnerId(string seed) =>
        new(IdGen.Generate($"BindingTests:{seed}:owner"));

    [Fact]
    public void Bind_RaisesCapitalBoundEvent()
    {
        var id = NewId("Bind_Valid");
        var accountId = NewAccountId("Bind_Valid");
        var ownerId = NewOwnerId("Bind_Valid");

        var aggregate = BindingAggregate.Bind(id, accountId, ownerId, OwnershipType.Individual, T0);

        var evt = Assert.IsType<CapitalBoundEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.BindingId);
        Assert.Equal(OwnershipType.Individual, evt.OwnershipType);
    }

    [Fact]
    public void Bind_SetsStatusToActive()
    {
        var aggregate = BindingAggregate.Bind(NewId("State"), NewAccountId("State"), NewOwnerId("State"), OwnershipType.Corporate, T0);

        Assert.Equal(BindingStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Bind_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var accountId = NewAccountId("Stable");
        var ownerId = NewOwnerId("Stable");

        var b1 = BindingAggregate.Bind(id, accountId, ownerId, OwnershipType.Individual, T0);
        var b2 = BindingAggregate.Bind(id, accountId, ownerId, OwnershipType.Individual, T0);

        Assert.Equal(
            ((CapitalBoundEvent)b1.DomainEvents[0]).BindingId.Value,
            ((CapitalBoundEvent)b2.DomainEvents[0]).BindingId.Value);
    }

    [Fact]
    public void TransferOwnership_SetsStatusToTransferred()
    {
        var aggregate = BindingAggregate.Bind(NewId("Transfer"), NewAccountId("Transfer"), NewOwnerId("Transfer_old"), OwnershipType.Individual, T0);
        aggregate.ClearDomainEvents();
        var newOwner = NewOwnerId("Transfer_new");

        aggregate.TransferOwnership(newOwner, OwnershipType.Joint, T1);

        Assert.IsType<OwnershipTransferredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(BindingStatus.Transferred, aggregate.Status);
    }

    [Fact]
    public void Release_SetsStatusToReleased()
    {
        var aggregate = BindingAggregate.Bind(NewId("Release"), NewAccountId("Release"), NewOwnerId("Release"), OwnershipType.Corporate, T0);
        aggregate.ClearDomainEvents();

        aggregate.Release(T1);

        Assert.IsType<BindingReleasedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(BindingStatus.Released, aggregate.Status);
    }

    [Fact]
    public void TransferOwnership_AfterRelease_Throws()
    {
        var aggregate = BindingAggregate.Bind(NewId("Transfer_Released"), NewAccountId("TR_acc"), NewOwnerId("TR_old"), OwnershipType.Individual, T0);
        aggregate.Release(T0);

        Assert.ThrowsAny<Exception>(() => aggregate.TransferOwnership(NewOwnerId("TR_new"), OwnershipType.Joint, T1));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var accountId = NewAccountId("History");
        var ownerId = NewOwnerId("History");

        var history = new object[]
        {
            new CapitalBoundEvent(id, accountId.Value, ownerId.Value, OwnershipType.Trust, T0)
        };

        var aggregate = (BindingAggregate)Activator.CreateInstance(typeof(BindingAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.BindingId);
        Assert.Equal(BindingStatus.Active, aggregate.Status);
        Assert.Equal(OwnershipType.Trust, aggregate.OwnershipType);
        Assert.Empty(aggregate.DomainEvents);
    }
}
