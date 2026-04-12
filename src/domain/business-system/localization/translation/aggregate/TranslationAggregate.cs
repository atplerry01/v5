namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public sealed class TranslationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TranslationId Id { get; private set; }
    public TranslationStatus Status { get; private set; }
    public TranslationKey Key { get; private set; }
    public int Version { get; private set; }

    private TranslationAggregate() { }

    public static TranslationAggregate Create(TranslationId id, TranslationKey key)
    {
        var aggregate = new TranslationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TranslationCreatedEvent(id, key);
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
            throw TranslationErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TranslationActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TranslationErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new TranslationSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reactivate()
    {
        ValidateBeforeChange();

        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TranslationErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new TranslationReactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TranslationErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new TranslationArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TranslationCreatedEvent @event)
    {
        Id = @event.TranslationId;
        Key = @event.Key;
        Status = TranslationStatus.Draft;
        Version++;
    }

    private void Apply(TranslationActivatedEvent @event)
    {
        Status = TranslationStatus.Active;
        Version++;
    }

    private void Apply(TranslationSuspendedEvent @event)
    {
        Status = TranslationStatus.Suspended;
        Version++;
    }

    private void Apply(TranslationReactivatedEvent @event)
    {
        Status = TranslationStatus.Active;
        Version++;
    }

    private void Apply(TranslationArchivedEvent @event)
    {
        Status = TranslationStatus.Archived;
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
            throw TranslationErrors.MissingId();

        if (Key == default)
            throw TranslationErrors.InvalidTranslationKey();

        if (!Enum.IsDefined(Status))
            throw TranslationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
