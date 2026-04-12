namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public sealed class LocaleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LocaleId Id { get; private set; }
    public LocaleStatus Status { get; private set; }
    public LocaleCode Code { get; private set; }
    public int Version { get; private set; }

    private LocaleAggregate() { }

    public static LocaleAggregate Create(LocaleId id, LocaleCode code)
    {
        var aggregate = new LocaleAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new LocaleCreatedEvent(id, code);
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
            throw LocaleErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new LocaleActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LocaleErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new LocaleDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LocaleCreatedEvent @event)
    {
        Id = @event.LocaleId;
        Code = @event.Code;
        Status = LocaleStatus.Draft;
        Version++;
    }

    private void Apply(LocaleActivatedEvent @event)
    {
        Status = LocaleStatus.Active;
        Version++;
    }

    private void Apply(LocaleDeactivatedEvent @event)
    {
        Status = LocaleStatus.Deactivated;
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
            throw LocaleErrors.MissingId();

        if (Code == default)
            throw LocaleErrors.InvalidLocaleCode();

        if (!Enum.IsDefined(Status))
            throw LocaleErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
