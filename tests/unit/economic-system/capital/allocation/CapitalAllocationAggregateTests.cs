using Whycespace.Domain.EconomicSystem.Capital.Allocation;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Capital.Allocation;

public sealed class CapitalAllocationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static AllocationId NewAllocId(string seed) =>
        new(IdGen.Generate($"CapitalAllocationTests:{seed}:allocation"));

    private static AccountId NewAccountId(string seed) =>
        new(IdGen.Generate($"CapitalAllocationTests:{seed}:account"));

    private static TargetId NewTargetId(string seed) =>
        new(IdGen.Generate($"CapitalAllocationTests:{seed}:target"));

    private static CapitalAllocationAggregate AllocateNew(string seed)
    {
        var aggregate = (CapitalAllocationAggregate)Activator.CreateInstance(typeof(CapitalAllocationAggregate), nonPublic: true)!;
        aggregate.Allocate(NewAllocId(seed), NewAccountId(seed), NewTargetId(seed), new Amount(1000m), Usd, T0);
        return aggregate;
    }

    [Fact]
    public void Allocate_RaisesAllocationCreatedEvent()
    {
        var allocId = NewAllocId("Allocate_Valid");
        var accountId = NewAccountId("Allocate_Valid");
        var targetId = NewTargetId("Allocate_Valid");
        var aggregate = (CapitalAllocationAggregate)Activator.CreateInstance(typeof(CapitalAllocationAggregate), nonPublic: true)!;

        aggregate.Allocate(allocId, accountId, targetId, new Amount(2000m), Usd, T0);

        var evt = Assert.IsType<AllocationCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(allocId, evt.AllocationId);
        Assert.Equal(2000m, evt.Amount.Value);
    }

    [Fact]
    public void Allocate_SetsPendingStatus()
    {
        var aggregate = AllocateNew("State");

        Assert.Equal(AllocationStatus.Pending, aggregate.Status);
        Assert.Equal(1000m, aggregate.Amount.Value);
    }

    [Fact]
    public void Allocate_ZeroAmount_Throws()
    {
        var aggregate = (CapitalAllocationAggregate)Activator.CreateInstance(typeof(CapitalAllocationAggregate), nonPublic: true)!;

        Assert.ThrowsAny<Exception>(() =>
            aggregate.Allocate(NewAllocId("Zero"), NewAccountId("Zero"), NewTargetId("Zero"), new Amount(0m), Usd, T0));
    }

    [Fact]
    public void Complete_FromPending_SetsStatusToCompleted()
    {
        var aggregate = AllocateNew("Complete");
        aggregate.ClearDomainEvents();

        aggregate.Complete(T1);

        Assert.IsType<AllocationCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AllocationStatus.Completed, aggregate.Status);
    }

    [Fact]
    public void Release_FromPending_SetsStatusToReleased()
    {
        var aggregate = AllocateNew("Release");
        aggregate.ClearDomainEvents();

        aggregate.Release(T1);

        Assert.IsType<AllocationReleasedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AllocationStatus.Released, aggregate.Status);
    }

    [Fact]
    public void Complete_AfterRelease_Throws()
    {
        var aggregate = AllocateNew("Complete_Released");
        aggregate.Release(T0);

        Assert.ThrowsAny<Exception>(() => aggregate.Complete(T1));
    }

    [Fact]
    public void Release_AfterComplete_Throws()
    {
        var aggregate = AllocateNew("Release_Completed");
        aggregate.Complete(T0);

        Assert.ThrowsAny<Exception>(() => aggregate.Release(T1));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var allocId = NewAllocId("History");
        var accountId = NewAccountId("History");
        var targetId = NewTargetId("History");

        var history = new object[]
        {
            new AllocationCreatedEvent(allocId, accountId.Value, targetId, new Amount(5000m), Usd, T0)
        };

        var aggregate = (CapitalAllocationAggregate)Activator.CreateInstance(typeof(CapitalAllocationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(allocId, aggregate.AllocationId);
        Assert.Equal(AllocationStatus.Pending, aggregate.Status);
        Assert.Equal(5000m, aggregate.Amount.Value);
        Assert.Empty(aggregate.DomainEvents);
    }
}
