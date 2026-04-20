using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed class TariffAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TariffId Id { get; private set; }
    public PriceBookRef PriceBook { get; private set; }
    public TariffCode Code { get; private set; }
    public TariffName Name { get; private set; }
    public TariffStatus Status { get; private set; }
    public TimeWindow? Effective { get; private set; }
    public int Version { get; private set; }

    private TariffAggregate() { }

    public static TariffAggregate Create(
        TariffId id,
        PriceBookRef priceBook,
        TariffCode code,
        TariffName name)
    {
        var aggregate = new TariffAggregate();

        var @event = new TariffCreatedEvent(id, priceBook, code, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TariffErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TariffActivatedEvent(Id, effective);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TariffErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new TariffDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == TariffStatus.Archived)
            throw TariffErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new TariffArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TariffCreatedEvent @event)
    {
        Id = @event.TariffId;
        PriceBook = @event.PriceBook;
        Code = @event.Code;
        Name = @event.Name;
        Status = TariffStatus.Draft;
        Version++;
    }

    private void Apply(TariffActivatedEvent @event)
    {
        Effective = @event.Effective;
        Status = TariffStatus.Active;
        Version++;
    }

    private void Apply(TariffDeprecatedEvent @event)
    {
        Status = TariffStatus.Deprecated;
        Version++;
    }

    private void Apply(TariffArchivedEvent @event)
    {
        Status = TariffStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
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
