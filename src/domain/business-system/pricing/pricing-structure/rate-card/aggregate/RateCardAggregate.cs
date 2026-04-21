using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public sealed class RateCardAggregate : AggregateRoot
{
    private readonly Dictionary<string, RateEntry> _entries = new();

    public RateCardId Id { get; private set; }
    public PriceBookRef PriceBook { get; private set; }
    public RateCardName Name { get; private set; }
    public RateCardStatus Status { get; private set; }
    public TimeWindow? Effective { get; private set; }
    public IReadOnlyDictionary<string, RateEntry> Entries => _entries;

    public static RateCardAggregate Create(
        RateCardId id,
        PriceBookRef priceBook,
        RateCardName name)
    {
        var aggregate = new RateCardAggregate();
        if (aggregate.Version >= 0)
            throw RateCardErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new RateCardCreatedEvent(id, priceBook, name));
        return aggregate;
    }

    public void AddEntry(RateEntry entry)
    {
        EnsureDraft();

        if (_entries.ContainsKey(entry.Code))
            throw RateCardErrors.RateEntryAlreadyPresent(entry.Code);

        RaiseDomainEvent(new RateEntryAddedEvent(Id, entry));
    }

    public void RemoveEntry(string code)
    {
        EnsureDraft();

        if (!_entries.ContainsKey(code))
            throw RateCardErrors.RateEntryNotPresent(code);

        RaiseDomainEvent(new RateEntryRemovedEvent(Id, code));
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

        RaiseDomainEvent(new RateCardActivatedEvent(Id, effective));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RateCardErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new RateCardDeprecatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == RateCardStatus.Archived)
            throw RateCardErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new RateCardArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RateCardCreatedEvent e:
                Id = e.RateCardId;
                PriceBook = e.PriceBook;
                Name = e.Name;
                Status = RateCardStatus.Draft;
                break;
            case RateEntryAddedEvent e:
                _entries[e.Entry.Code] = e.Entry;
                break;
            case RateEntryRemovedEvent e:
                _entries.Remove(e.Code);
                break;
            case RateCardActivatedEvent e:
                Effective = e.Effective;
                Status = RateCardStatus.Active;
                break;
            case RateCardDeprecatedEvent:
                Status = RateCardStatus.Deprecated;
                break;
            case RateCardArchivedEvent:
                Status = RateCardStatus.Archived;
                break;
        }
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

    protected override void EnsureInvariants()
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
