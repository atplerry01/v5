using Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

namespace Whycespace.Tests.Unit.ConstitutionalSystem.Chain.EvidenceRecord;

public sealed class EvidenceRecordAggregateTests
{
    private static readonly Guid SampleId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static EvidenceDescriptor ValidDescriptor() => new(
        CorrelationId: Guid.NewGuid(),
        AnchorRecordId: Guid.NewGuid(),
        EvidenceType: EvidenceType.Command,
        ActorId: "actor@system.test",
        SubjectId: "registry:abc123",
        PolicyHash: "policy-hash-001");

    [Fact]
    public void Record_ValidInputs_RaisesEvidenceRecordCreatedEvent()
    {
        var aggregate = EvidenceRecordAggregate.Record(
            new EvidenceRecordId(SampleId), ValidDescriptor(), Now);

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<EvidenceRecordCreatedEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(EvidenceRecordStatus.Active, aggregate.Status);
        Assert.Equal(SampleId, aggregate.Id.Value);
    }

    [Fact]
    public void Record_EmptyActorId_Throws()
    {
        var descriptor = ValidDescriptor() with { ActorId = "" };
        Assert.Throws<ArgumentException>(
            () => EvidenceRecordAggregate.Record(new EvidenceRecordId(SampleId), descriptor, Now));
    }

    [Fact]
    public void Record_EmptySubjectId_Throws()
    {
        var descriptor = ValidDescriptor() with { SubjectId = "" };
        Assert.Throws<ArgumentException>(
            () => EvidenceRecordAggregate.Record(new EvidenceRecordId(SampleId), descriptor, Now));
    }

    [Fact]
    public void Archive_FromActive_RaisesEvidenceRecordArchivedEvent()
    {
        var aggregate = EvidenceRecordAggregate.Record(
            new EvidenceRecordId(SampleId), ValidDescriptor(), Now);
        aggregate.ClearDomainEvents();

        aggregate.Archive(Now.AddHours(1));

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<EvidenceRecordArchivedEvent>(aggregate.DomainEvents[0]);
        Assert.Equal(EvidenceRecordStatus.Archived, aggregate.Status);
        Assert.NotNull(aggregate.ArchivedAt);
    }

    [Fact]
    public void Archive_AlreadyArchived_Throws()
    {
        var aggregate = EvidenceRecordAggregate.Record(
            new EvidenceRecordId(SampleId), ValidDescriptor(), Now);
        aggregate.Archive(Now.AddHours(1));

        Assert.Throws<InvalidOperationException>(() => aggregate.Archive(Now.AddHours(2)));
    }

    [Fact]
    public void Record_DescriptorPreservedOnEvent()
    {
        var descriptor = ValidDescriptor();
        var aggregate = EvidenceRecordAggregate.Record(
            new EvidenceRecordId(SampleId), descriptor, Now);

        var created = aggregate.DomainEvents.OfType<EvidenceRecordCreatedEvent>().Single();
        Assert.Equal(descriptor.ActorId, created.Descriptor.ActorId);
        Assert.Equal(descriptor.EvidenceType, created.Descriptor.EvidenceType);
        Assert.Equal(descriptor.PolicyHash, created.Descriptor.PolicyHash);
    }
}
