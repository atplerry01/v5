namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public sealed class ListingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ListingId Id { get; private set; }
    public ListingOwnerId OwnerId { get; private set; }
    public ListingItemReference ItemReference { get; private set; }
    public string ListingTitle { get; private set; } = null!;
    public ListingStatus Status { get; private set; }
    public int Version { get; private set; }

    private ListingAggregate() { }

    public static ListingAggregate Create(ListingId id, ListingOwnerId ownerId, ListingItemReference itemReference, string listingTitle)
    {
        if (string.IsNullOrWhiteSpace(listingTitle))
            throw new ArgumentException("Listing title must not be empty.", nameof(listingTitle));

        var aggregate = new ListingAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ListingCreatedEvent(id, ownerId, itemReference, listingTitle);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateListingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ListingErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ListingActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateListingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ListingErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new ListingDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ListingCreatedEvent @event)
    {
        Id = @event.ListingId;
        OwnerId = @event.OwnerId;
        ItemReference = @event.ItemReference;
        ListingTitle = @event.ListingTitle;
        Status = ListingStatus.Draft;
        Version++;
    }

    private void Apply(ListingActivatedEvent @event)
    {
        Status = ListingStatus.Active;
        Version++;
    }

    private void Apply(ListingDeactivatedEvent @event)
    {
        Status = ListingStatus.Inactive;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ListingErrors.MissingId();

        if (OwnerId == default)
            throw ListingErrors.MissingOwnerId();

        if (ItemReference == default)
            throw ListingErrors.MissingItemReference();

        if (!Enum.IsDefined(Status))
            throw ListingErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
