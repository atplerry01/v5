using Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

namespace Whycespace.Tests.Unit.ConstitutionalSystem.Chain.Ledger;

public sealed class LedgerAggregateTests
{
    private static readonly Guid SampleId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public void Open_ValidInputs_RaisesLedgerOpenedEvent()
    {
        var aggregate = LedgerAggregate.Open(
            new LedgerId(SampleId),
            new LedgerDescriptor("platform-chain"),
            Now);

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<LedgerOpenedEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(LedgerStatus.Open, aggregate.Status);
        Assert.Equal(SampleId, aggregate.Id.Value);
        Assert.Equal("platform-chain", aggregate.Descriptor.LedgerName);
    }

    [Fact]
    public void Open_EmptyLedgerName_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => LedgerAggregate.Open(new LedgerId(SampleId), new LedgerDescriptor(""), Now));
    }

    [Fact]
    public void Seal_FromOpen_RaisesLedgerSealedEvent()
    {
        var aggregate = LedgerAggregate.Open(
            new LedgerId(SampleId), new LedgerDescriptor("platform-chain"), Now);
        aggregate.ClearDomainEvents();

        aggregate.Seal(Now.AddDays(1));

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<LedgerSealedEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(LedgerStatus.Sealed, aggregate.Status);
        Assert.NotNull(aggregate.SealedAt);
    }

    [Fact]
    public void Seal_AlreadySealed_Throws()
    {
        var aggregate = LedgerAggregate.Open(
            new LedgerId(SampleId), new LedgerDescriptor("platform-chain"), Now);
        aggregate.Seal(Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Seal(Now.AddDays(2)));
    }

    [Fact]
    public void Open_OpenedAtPreservedOnEvent()
    {
        var aggregate = LedgerAggregate.Open(
            new LedgerId(SampleId), new LedgerDescriptor("platform-chain"), Now);

        var opened = aggregate.DomainEvents.OfType<LedgerOpenedEvent>().Single();
        Assert.Equal(Now, opened.OpenedAt);
        Assert.Equal("platform-chain", opened.Descriptor.LedgerName);
    }
}
