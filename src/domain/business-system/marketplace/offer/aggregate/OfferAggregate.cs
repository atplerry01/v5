namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed class OfferAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public OfferId Id { get; private set; }
    public OfferListingReference ListingReference { get; private set; }
    public OfferTerms Terms { get; private set; }
    public OfferStatus Status { get; private set; }
    public int Version { get; private set; }

    private OfferAggregate() { }

    public static OfferAggregate Create(OfferId id, OfferListingReference listingReference, OfferTerms terms)
    {
        if (terms is null)
            throw new ArgumentNullException(nameof(terms));

        var aggregate = new OfferAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new OfferCreatedEvent(id, listingReference, terms.Description);
        aggregate.Apply(@event, terms);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Accept()
    {
        ValidateBeforeChange();

        var specification = new CanAcceptOfferSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OfferErrors.InvalidStateTransition(Status, nameof(Accept));

        var @event = new OfferAcceptedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject()
    {
        ValidateBeforeChange();

        var specification = new CanRejectOfferSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OfferErrors.InvalidStateTransition(Status, nameof(Reject));

        var @event = new OfferRejectedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Withdraw()
    {
        ValidateBeforeChange();

        var specification = new CanWithdrawOfferSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OfferErrors.InvalidStateTransition(Status, nameof(Withdraw));

        var @event = new OfferWithdrawnEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(OfferCreatedEvent @event, OfferTerms terms)
    {
        Id = @event.OfferId;
        ListingReference = @event.ListingReference;
        Terms = terms;
        Status = OfferStatus.Pending;
        Version++;
    }

    private void Apply(OfferAcceptedEvent @event)
    {
        Status = OfferStatus.Accepted;
        Version++;
    }

    private void Apply(OfferRejectedEvent @event)
    {
        Status = OfferStatus.Rejected;
        Version++;
    }

    private void Apply(OfferWithdrawnEvent @event)
    {
        Status = OfferStatus.Withdrawn;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw OfferErrors.MissingId();

        if (ListingReference == default)
            throw OfferErrors.MissingListingReference();

        if (Terms is null)
            throw OfferErrors.MissingTerms();

        if (!Enum.IsDefined(Status))
            throw OfferErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
