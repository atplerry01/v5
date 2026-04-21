using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed class PriceBookAggregate : AggregateRoot
{
    public PriceBookId Id { get; private set; }
    public PriceBookName Name { get; private set; }
    public PriceBookStatus Status { get; private set; }
    public PriceBookScopeRef? Scope { get; private set; }
    public TimeWindow? Effective { get; private set; }

    public static PriceBookAggregate Create(
        PriceBookId id,
        PriceBookName name,
        PriceBookScopeRef? scope = null,
        TimeWindow? effective = null)
    {
        var aggregate = new PriceBookAggregate();
        if (aggregate.Version >= 0)
            throw PriceBookErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new PriceBookCreatedEvent(id, name, scope, effective));
        return aggregate;
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new PriceBookActivatedEvent(Id, effective));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new PriceBookDeprecatedEvent(Id));
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new PriceBookArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PriceBookCreatedEvent e:
                Id = e.PriceBookId;
                Name = e.Name;
                Scope = e.Scope;
                Effective = e.Effective;
                Status = PriceBookStatus.Draft;
                break;
            case PriceBookActivatedEvent e:
                Effective = e.Effective;
                Status = PriceBookStatus.Active;
                break;
            case PriceBookDeprecatedEvent:
                Status = PriceBookStatus.Deprecated;
                break;
            case PriceBookArchivedEvent:
                Status = PriceBookStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw PriceBookErrors.MissingId();

        if (Status == PriceBookStatus.Active && Effective is null)
            throw PriceBookErrors.EffectiveWindowRequiredForActivation();

        if (!Enum.IsDefined(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, "validate");
    }
}
