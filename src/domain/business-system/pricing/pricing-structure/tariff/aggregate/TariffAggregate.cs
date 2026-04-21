using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed class TariffAggregate : AggregateRoot
{
    public TariffId Id { get; private set; }
    public PriceBookRef PriceBook { get; private set; }
    public TariffCode Code { get; private set; }
    public TariffName Name { get; private set; }
    public TariffStatus Status { get; private set; }
    public TimeWindow? Effective { get; private set; }

    public static TariffAggregate Create(
        TariffId id,
        PriceBookRef priceBook,
        TariffCode code,
        TariffName name)
    {
        var aggregate = new TariffAggregate();
        if (aggregate.Version >= 0)
            throw TariffErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new TariffCreatedEvent(id, priceBook, code, name));
        return aggregate;
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TariffErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new TariffActivatedEvent(Id, effective));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TariffErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new TariffDeprecatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == TariffStatus.Archived)
            throw TariffErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new TariffArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TariffCreatedEvent e:
                Id = e.TariffId;
                PriceBook = e.PriceBook;
                Code = e.Code;
                Name = e.Name;
                Status = TariffStatus.Draft;
                break;
            case TariffActivatedEvent e:
                Effective = e.Effective;
                Status = TariffStatus.Active;
                break;
            case TariffDeprecatedEvent:
                Status = TariffStatus.Deprecated;
                break;
            case TariffArchivedEvent:
                Status = TariffStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw TariffErrors.MissingId();

        if (PriceBook == default)
            throw TariffErrors.MissingPriceBookRef();

        if (Status == TariffStatus.Active && Effective is null)
            throw TariffErrors.ActivationRequiresEffectiveWindow();

        if (!Enum.IsDefined(Status))
            throw TariffErrors.InvalidStateTransition(Status, "validate");
    }
}
