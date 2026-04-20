using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed class RateCardAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly Dictionary<string, RateEntry> _entries = new();

    public RateCardId Id { get; private set; }
    public PriceBookRef PriceBook { get; private set; }
    public RateCardName Name { get; private set; }
    public RateCardStatus Status { get; private set; }
    public TimeWindow? Effective { get; private set; }
    public IReadOnlyDictionary<string, RateEntry> Entries => _entries;
    public int Version { get; private set; }

    private RateCardAggregate() { }

    public static RateCardAggregate Create(
        RateCardId id,
        PriceBookRef priceBook,
        RateCardName name)
    {
        var aggregate = new RateCardAggregate();

        var @event = new RateCardCreatedEvent(id, priceBook, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddEntry(RateEntry entry)
    {
        EnsureDraft();

        if (_entries.ContainsKey(entry.Code))
            throw RateCardErrors.RateEntryAlreadyPresent(entry.Code);

        var @event = new RateEntryAddedEvent(Id, entry);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RemoveEntry(string code)
    {
        EnsureDraft();

        if (!_entries.ContainsKey(code))
            throw RateCardErrors.RateEntryNotPresent(code);

        var @event = new RateEntryRemovedEvent(Id, code);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status, _entries.Count))
        {
            if (Status != RateCardStatus.Draft)
                throw RateCardErrors.InvalidStateTransition(Status, nameof(Activate));

            throw RateCardErrors.ActivationRequiresEntries();
        }

        var @event = new RateCardActivatedEvent(Id, effective);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RateCardErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new RateCardDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == RateCardStatus.Archived)
            throw RateCardErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new RateCardArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RateCardCreatedEvent @event)
    {
        Id = @event.RateCardId;
        PriceBook = @event.PriceBook;
        Name = @event.Name;
        Status = RateCardStatus.Draft;
        Version++;
    }

    private void Apply(RateEntryAddedEvent @event)
    {
        _entries[@event.Entry.Code] = @event.Entry;
        Version++;
    }

    private void Apply(RateEntryRemovedEvent @event)
    {
        _entries.Remove(@event.Code);
        Version++;
    }

    private void Apply(RateCardActivatedEvent @event)
    {
        Effective = @event.Effective;
        Status = RateCardStatus.Active;
        Version++;
    }

    private void Apply(RateCardDeprecatedEvent @event)
    {
        Status = RateCardStatus.Deprecated;
        Version++;
    }

    private void Apply(RateCardArchivedEvent @event)
    {
        Status = RateCardStatus.Archived;
        Version++;
    }

    private void EnsureDraft()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
        {
            if (Status == RateCardStatus.Archived)
                throw RateCardErrors.ArchivedImmutable(Id);
            throw RateCardErrors.InvalidStateTransition(Status, "mutate");
        }
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw RateCardErrors.MissingId();

        if (PriceBook == default)
            throw RateCardErrors.MissingPriceBookRef();

        if (Status == RateCardStatus.Active && Effective is null)
            throw RateCardErrors.ActivationRequiresEffectiveWindow();

        if (!Enum.IsDefined(Status))
            throw RateCardErrors.InvalidStateTransition(Status, "validate");
    }
}
