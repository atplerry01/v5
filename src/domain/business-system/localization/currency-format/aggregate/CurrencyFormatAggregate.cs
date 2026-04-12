namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public sealed class CurrencyFormatAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CurrencyFormatId Id { get; private set; }
    public CurrencyFormatStatus Status { get; private set; }
    public CurrencyCode Code { get; private set; }
    public int Version { get; private set; }

    private CurrencyFormatAggregate() { }

    public static CurrencyFormatAggregate Create(CurrencyFormatId id, CurrencyCode code)
    {
        var aggregate = new CurrencyFormatAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CurrencyFormatCreatedEvent(id, code);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CurrencyFormatErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new CurrencyFormatActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CurrencyFormatErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new CurrencyFormatDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CurrencyFormatCreatedEvent @event)
    {
        Id = @event.CurrencyFormatId;
        Code = @event.Code;
        Status = CurrencyFormatStatus.Draft;
        Version++;
    }

    private void Apply(CurrencyFormatActivatedEvent @event)
    {
        Status = CurrencyFormatStatus.Active;
        Version++;
    }

    private void Apply(CurrencyFormatDeactivatedEvent @event)
    {
        Status = CurrencyFormatStatus.Deactivated;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CurrencyFormatErrors.MissingId();

        if (Code == default)
            throw CurrencyFormatErrors.InvalidCurrencyCode();

        if (!Enum.IsDefined(Status))
            throw CurrencyFormatErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
