using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed class UsageRightAggregate : AggregateRoot
{
    private readonly List<UsageRecord> _usageRecords = new();

    public UsageRightId Id { get; private set; }
    public UsageRightSubjectId SubjectId { get; private set; }
    public UsageRightReferenceId ReferenceId { get; private set; }
    public UsageRightStatus Status { get; private set; }
    public int TotalUnits { get; private set; }
    public int TotalUsed { get; private set; }
    public IReadOnlyList<UsageRecord> UsageRecords => _usageRecords.AsReadOnly();
    public int Remaining => TotalUnits - TotalUsed;

    public static UsageRightAggregate Create(UsageRightId id, UsageRightSubjectId subjectId, UsageRightReferenceId referenceId, int totalUnits)
    {
        Guard.Against(totalUnits <= 0, "Total units must be greater than zero.");

        var aggregate = new UsageRightAggregate();
        if (aggregate.Version >= 0)
            throw UsageRightErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new UsageRightCreatedEvent(id, subjectId, referenceId, totalUnits));
        return aggregate;
    }

    public void Use(UsageRecord record)
    {
        Guard.Against(record is null, "UsageRecord must not be null.");

        var specification = new CanUseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UsageRightErrors.InvalidStateTransition(Status, nameof(Use));

        if (record!.UnitsUsed > Remaining)
            throw UsageRightErrors.UsageExceedsAvailable(record.UnitsUsed, Remaining);

        RaiseDomainEvent(new UsageRightUsedEvent(Id, record.RecordId, record.UnitsUsed));
    }

    public void Consume()
    {
        var specification = new CanConsumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UsageRightErrors.InvalidStateTransition(Status, nameof(Consume));

        if (Remaining > 0)
            throw UsageRightErrors.UsageRemaining(Remaining);

        RaiseDomainEvent(new UsageRightConsumedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case UsageRightCreatedEvent e:
                Id = e.UsageRightId;
                SubjectId = e.SubjectId;
                ReferenceId = e.ReferenceId;
                TotalUnits = e.TotalUnits;
                TotalUsed = 0;
                Status = UsageRightStatus.Available;
                break;
            case UsageRightUsedEvent e:
                _usageRecords.Add(new UsageRecord(e.RecordId, e.UnitsUsed));
                TotalUsed += e.UnitsUsed;
                Status = UsageRightStatus.InUse;
                break;
            case UsageRightConsumedEvent:
                Status = UsageRightStatus.Consumed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw UsageRightErrors.MissingId();

        if (SubjectId == default)
            throw UsageRightErrors.MissingSubjectId();

        if (ReferenceId == default)
            throw UsageRightErrors.MissingReferenceId();

        if (TotalUnits <= 0)
            throw UsageRightErrors.InvalidTotalUnits();

        if (!Enum.IsDefined(Status))
            throw UsageRightErrors.InvalidStateTransition(Status, "validate");
    }
}
