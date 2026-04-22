using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

public sealed class EvidenceRecordAggregate : AggregateRoot
{
    public EvidenceRecordId Id { get; private set; }
    public EvidenceDescriptor Descriptor { get; private set; } = null!;
    public EvidenceRecordStatus Status { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }

    private EvidenceRecordAggregate() { }

    public static EvidenceRecordAggregate Record(
        EvidenceRecordId id,
        EvidenceDescriptor descriptor,
        DateTimeOffset recordedAt)
    {
        if (string.IsNullOrWhiteSpace(descriptor.ActorId))
            throw EvidenceRecordErrors.EmptyActorId();
        if (string.IsNullOrWhiteSpace(descriptor.SubjectId))
            throw EvidenceRecordErrors.EmptySubjectId();

        var aggregate = new EvidenceRecordAggregate();
        aggregate.RaiseDomainEvent(new EvidenceRecordCreatedEvent(id, descriptor, recordedAt));
        return aggregate;
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (!EvidenceNotArchivedSpecification.IsSatisfiedBy(Status))
            throw EvidenceRecordErrors.AlreadyArchived();

        RaiseDomainEvent(new EvidenceRecordArchivedEvent(Id, archivedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EvidenceRecordCreatedEvent e:
                Id = e.EvidenceRecordId.Value == Guid.Empty
                    ? new EvidenceRecordId(AggregateIdentity)
                    : e.EvidenceRecordId;
                Descriptor = e.Descriptor;
                RecordedAt = e.RecordedAt;
                Status = EvidenceRecordStatus.Active;
                break;

            case EvidenceRecordArchivedEvent e:
                Status = EvidenceRecordStatus.Archived;
                ArchivedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value == Guid.Empty, "EvidenceRecord must have a valid Id.");
        Guard.Against(Descriptor is null, "EvidenceRecord must have a descriptor.");
    }
}
