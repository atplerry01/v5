using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed class QuoteAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public QuoteId Id { get; private set; }
    public QuoteBasisRef QuoteBasis { get; private set; }
    public QuoteReference Reference { get; private set; }
    public QuoteStatus Status { get; private set; }
    public TimeWindow? Validity { get; private set; }
    public int Version { get; private set; }

    private QuoteAggregate() { }

    public static QuoteAggregate Create(
        QuoteId id,
        QuoteBasisRef quoteBasis,
        QuoteReference reference)
    {
        var aggregate = new QuoteAggregate();

        var @event = new QuoteCreatedEvent(id, quoteBasis, reference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Issue(TimeWindow validity)
    {
        var specification = new CanIssueSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.InvalidStateTransition(Status, nameof(Issue));

        if (!validity.IsClosed)
            throw QuoteErrors.IssuanceRequiresValidity();

        var @event = new QuoteIssuedEvent(Id, validity);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Accept(DateTimeOffset acceptedAt)
    {
        var specification = new CanAcceptSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.AlreadyTerminal(Id, Status);

        if (Validity.HasValue && Validity.Value.IsExpiredAt(acceptedAt))
            throw QuoteErrors.InvalidStateTransition(Status, $"{nameof(Accept)}-after-expiry");

        var @event = new QuoteAcceptedEvent(Id, acceptedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject(DateTimeOffset rejectedAt)
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.AlreadyTerminal(Id, Status);

        var @event = new QuoteRejectedEvent(Id, rejectedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire(DateTimeOffset expiredAt)
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.InvalidStateTransition(Status, nameof(Expire));

        if (Validity is null)
            throw QuoteErrors.InvalidStateTransition(Status, $"{nameof(Expire)}-without-validity");

        if (!Validity.Value.IsExpiredAt(expiredAt))
            throw QuoteErrors.ExpiryNotReached();

        var @event = new QuoteExpiredEvent(Id, expiredAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(QuoteCreatedEvent @event)
    {
        Id = @event.QuoteId;
        QuoteBasis = @event.QuoteBasis;
        Reference = @event.Reference;
        Status = QuoteStatus.Draft;
        Version++;
    }

    private void Apply(QuoteIssuedEvent @event)
    {
        Validity = @event.Validity;
        Status = QuoteStatus.Issued;
        Version++;
    }

    private void Apply(QuoteAcceptedEvent @event)
    {
        Status = QuoteStatus.Accepted;
        Version++;
    }

    private void Apply(QuoteRejectedEvent @event)
    {
        Status = QuoteStatus.Rejected;
        Version++;
    }

    private void Apply(QuoteExpiredEvent @event)
    {
        Status = QuoteStatus.Expired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw QuoteErrors.MissingId();

        if (QuoteBasis == default)
            throw QuoteErrors.MissingQuoteBasisRef();

        if (Status == QuoteStatus.Issued && Validity is null)
            throw QuoteErrors.IssuanceRequiresValidity();

        if (!Enum.IsDefined(Status))
            throw QuoteErrors.InvalidStateTransition(Status, "validate");
    }
}
