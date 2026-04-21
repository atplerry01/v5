using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed class PlanAggregate : AggregateRoot
{
    public PlanId Id { get; private set; }
    public PlanDescriptor Descriptor { get; private set; }
    public PlanStatus Status { get; private set; }

    // -- Factory ----------------------------------------------------------

    public static PlanAggregate Draft(
        PlanId id,
        PlanDescriptor descriptor)
    {
        var aggregate = new PlanAggregate();
        if (aggregate.Version >= 0)
            throw PlanErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new PlanDraftedEvent(id, descriptor));
        return aggregate;
    }

    // -- Activate ---------------------------------------------------------

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PlanErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new PlanActivatedEvent(Id));
    }

    // -- Deprecate --------------------------------------------------------

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PlanErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new PlanDeprecatedEvent(Id));
    }

    // -- Archive ----------------------------------------------------------

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PlanErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new PlanArchivedEvent(Id));
    }

    // -- Apply ------------------------------------------------------------

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PlanDraftedEvent e:
                Id = e.PlanId;
                Descriptor = e.Descriptor;
                Status = PlanStatus.Draft;
                break;
            case PlanActivatedEvent:
                Status = PlanStatus.Active;
                break;
            case PlanDeprecatedEvent:
                Status = PlanStatus.Deprecated;
                break;
            case PlanArchivedEvent:
                Status = PlanStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw PlanErrors.MissingId();

        if (Descriptor == default)
            throw PlanErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw PlanErrors.InvalidStateTransition(Status, "validate");
    }
}
