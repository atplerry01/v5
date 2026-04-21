using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed class QuoteAggregate : AggregateRoot
{
    public QuoteId Id { get; private set; }
    public QuoteBasisRef QuoteBasis { get; private set; }
    public QuoteReference Reference { get; private set; }
    public QuoteStatus Status { get; private set; }
    public TimeWindow? Validity { get; private set; }

    public static QuoteAggregate Create(
        QuoteId id,
        QuoteBasisRef quoteBasis,
        QuoteReference reference)
    {
        var aggregate = new QuoteAggregate();
        if (aggregate.Version >= 0)
            throw QuoteErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new QuoteCreatedEvent(id, quoteBasis, reference));
        return aggregate;
    }

    public void Issue(TimeWindow validity)
    {
        var specification = new CanIssueSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.InvalidStateTransition(Status, nameof(Issue));

        if (!validity.IsClosed)
            throw QuoteErrors.IssuanceRequiresValidity();

        RaiseDomainEvent(new QuoteIssuedEvent(Id, validity));
    }

    public void Accept(DateTimeOffset acceptedAt)
    {
        var specification = new CanAcceptSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.AlreadyTerminal(Id, Status);

        if (Validity.HasValue && Validity.Value.IsExpiredAt(acceptedAt))
            throw QuoteErrors.InvalidStateTransition(Status, $"{nameof(Accept)}-after-expiry");

        RaiseDomainEvent(new QuoteAcceptedEvent(Id, acceptedAt));
    }

    public void Reject(DateTimeOffset rejectedAt)
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new QuoteRejectedEvent(Id, rejectedAt));
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

        RaiseDomainEvent(new QuoteExpiredEvent(Id, expiredAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case QuoteCreatedEvent e:
                Id = e.QuoteId;
                QuoteBasis = e.QuoteBasis;
                Reference = e.Reference;
                Status = QuoteStatus.Draft;
                break;
            case QuoteIssuedEvent e:
                Validity = e.Validity;
                Status = QuoteStatus.Issued;
                break;
            case QuoteAcceptedEvent:
                Status = QuoteStatus.Accepted;
                break;
            case QuoteRejectedEvent:
                Status = QuoteStatus.Rejected;
                break;
            case QuoteExpiredEvent:
                Status = QuoteStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
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
