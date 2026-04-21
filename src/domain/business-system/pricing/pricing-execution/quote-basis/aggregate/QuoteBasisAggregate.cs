using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed class QuoteBasisAggregate : AggregateRoot
{
    public QuoteBasisId Id { get; private set; }
    public PriceBookRef PriceBook { get; private set; }
    public QuoteBasisContext Context { get; private set; }
    public QuoteBasisStatus Status { get; private set; }

    public static QuoteBasisAggregate Create(
        QuoteBasisId id,
        PriceBookRef priceBook,
        QuoteBasisContext context)
    {
        var aggregate = new QuoteBasisAggregate();
        if (aggregate.Version >= 0)
            throw QuoteBasisErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new QuoteBasisCreatedEvent(id, priceBook, context));
        return aggregate;
    }

    public void ReviseContext(QuoteBasisContext context)
    {
        var specification = new CanReviseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteBasisErrors.FinalizedImmutable(Id);

        RaiseDomainEvent(new QuoteBasisContextRevisedEvent(Id, context));
    }

    public void MarkFinalized()
    {
        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteBasisErrors.InvalidStateTransition(Status, nameof(MarkFinalized));

        RaiseDomainEvent(new QuoteBasisFinalizedEvent(Id));
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteBasisErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new QuoteBasisArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case QuoteBasisCreatedEvent e:
                Id = e.QuoteBasisId;
                PriceBook = e.PriceBook;
                Context = e.Context;
                Status = QuoteBasisStatus.Draft;
                break;
            case QuoteBasisContextRevisedEvent e:
                Context = e.Context;
                break;
            case QuoteBasisFinalizedEvent:
                Status = QuoteBasisStatus.Finalized;
                break;
            case QuoteBasisArchivedEvent:
                Status = QuoteBasisStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw QuoteBasisErrors.MissingId();

        if (PriceBook == default)
            throw QuoteBasisErrors.MissingPriceBookRef();

        if (!Enum.IsDefined(Status))
            throw QuoteBasisErrors.InvalidStateTransition(Status, "validate");
    }
}
