using Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

namespace Whycespace.Tests.Unit.ConstitutionalSystem.Chain.AnchorRecord;

public sealed class AnchorRecordAggregateTests
{
    private static readonly Guid SampleId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static AnchorDescriptor ValidDescriptor() => new(
        CorrelationId: Guid.NewGuid(),
        BlockHash: "abc123hash",
        EventHash: "def456hash",
        PreviousBlockHash: "prev789hash",
        DecisionHash: "policy001hash",
        Sequence: 0);

    [Fact]
    public void Record_ValidInputs_RaisesAnchorRecordCreatedEvent()
    {
        var aggregate = AnchorRecordAggregate.Record(
            new AnchorRecordId(SampleId), ValidDescriptor(), Now);

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<AnchorRecordCreatedEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(AnchorRecordStatus.Created, aggregate.Status);
        Assert.Equal(SampleId, aggregate.Id.Value);
    }

    [Fact]
    public void Record_EmptyBlockHash_Throws()
    {
        var descriptor = ValidDescriptor() with { BlockHash = "" };
        Assert.Throws<ArgumentException>(
            () => AnchorRecordAggregate.Record(new AnchorRecordId(SampleId), descriptor, Now));
    }

    [Fact]
    public void Record_NegativeSequence_Throws()
    {
        var descriptor = ValidDescriptor() with { Sequence = -1 };
        Assert.Throws<ArgumentException>(
            () => AnchorRecordAggregate.Record(new AnchorRecordId(SampleId), descriptor, Now));
    }

    [Fact]
    public void Seal_FromCreated_RaisesAnchorRecordSealedEvent()
    {
        var aggregate = AnchorRecordAggregate.Record(
            new AnchorRecordId(SampleId), ValidDescriptor(), Now);
        aggregate.ClearDomainEvents();

        aggregate.Seal(Now.AddSeconds(1));

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<AnchorRecordSealedEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(AnchorRecordStatus.Sealed, aggregate.Status);
        Assert.NotNull(aggregate.SealedAt);
    }

    [Fact]
    public void Seal_AlreadySealed_Throws()
    {
        var aggregate = AnchorRecordAggregate.Record(
            new AnchorRecordId(SampleId), ValidDescriptor(), Now);
        aggregate.Seal(Now.AddSeconds(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Seal(Now.AddSeconds(2)));
    }

    [Fact]
    public void Seal_SealedAt_IsPreservedOnEvent()
    {
        var sealedAt = Now.AddHours(1);
        var aggregate = AnchorRecordAggregate.Record(
            new AnchorRecordId(SampleId), ValidDescriptor(), Now);
        aggregate.Seal(sealedAt);

        var sealEvent = aggregate.DomainEvents.OfType<AnchorRecordSealedEvent>().Single();
        Assert.Equal(sealedAt, sealEvent.SealedAt);
    }
}
