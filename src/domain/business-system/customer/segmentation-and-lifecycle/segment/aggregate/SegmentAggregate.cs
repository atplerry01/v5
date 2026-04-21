using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed class SegmentAggregate : AggregateRoot
{
    public SegmentId Id { get; private set; }
    public SegmentCode Code { get; private set; }
    public SegmentName Name { get; private set; }
    public SegmentType Type { get; private set; }
    public SegmentCriteria Criteria { get; private set; }
    public SegmentStatus Status { get; private set; }

    public static SegmentAggregate Create(
        SegmentId id,
        SegmentCode code,
        SegmentName name,
        SegmentType type,
        SegmentCriteria criteria)
    {
        var aggregate = new SegmentAggregate();
        if (aggregate.Version >= 0)
            throw SegmentErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SegmentCreatedEvent(id, code, name, type, criteria));
        return aggregate;
    }

    public void Update(SegmentName name, SegmentCriteria criteria)
    {
        EnsureMutable();
        RaiseDomainEvent(new SegmentUpdatedEvent(Id, name, criteria));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SegmentErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new SegmentActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == SegmentStatus.Archived)
            throw SegmentErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new SegmentArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SegmentCreatedEvent e:
                Id = e.SegmentId;
                Code = e.Code;
                Name = e.Name;
                Type = e.Type;
                Criteria = e.Criteria;
                Status = SegmentStatus.Draft;
                break;
            case SegmentUpdatedEvent e:
                Name = e.Name;
                Criteria = e.Criteria;
                break;
            case SegmentActivatedEvent:
                Status = SegmentStatus.Active;
                break;
            case SegmentArchivedEvent:
                Status = SegmentStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SegmentErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SegmentErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SegmentErrors.InvalidStateTransition(Status, "validate");
    }
}
