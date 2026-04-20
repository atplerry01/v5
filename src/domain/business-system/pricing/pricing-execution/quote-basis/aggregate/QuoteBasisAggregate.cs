using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed class QuoteBasisAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public QuoteBasisId Id { get; private set; }
    public PriceBookRef PriceBook { get; private set; }
    public QuoteBasisContext Context { get; private set; }
    public QuoteBasisStatus Status { get; private set; }
    public int Version { get; private set; }

    private QuoteBasisAggregate() { }

    public static QuoteBasisAggregate Create(
        QuoteBasisId id,
        PriceBookRef priceBook,
        QuoteBasisContext context)
    {
        var aggregate = new QuoteBasisAggregate();

        var @event = new QuoteBasisCreatedEvent(id, priceBook, context);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void ReviseContext(QuoteBasisContext context)
    {
        var specification = new CanReviseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteBasisErrors.FinalizedImmutable(Id);

        var @event = new QuoteBasisContextRevisedEvent(Id, context);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkFinalized()
    {
        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteBasisErrors.InvalidStateTransition(Status, nameof(MarkFinalized));

        var @event = new QuoteBasisFinalizedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuoteBasisErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new QuoteBasisArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(QuoteBasisCreatedEvent @event)
    {
        Id = @event.QuoteBasisId;
        PriceBook = @event.PriceBook;
        Context = @event.Context;
        Status = QuoteBasisStatus.Draft;
        Version++;
    }

    private void Apply(QuoteBasisContextRevisedEvent @event)
    {
        Context = @event.Context;
        Version++;
    }

    private void Apply(QuoteBasisFinalizedEvent @event)
    {
        Status = QuoteBasisStatus.Finalized;
        Version++;
    }

    private void Apply(QuoteBasisArchivedEvent @event)
    {
        Status = QuoteBasisStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw QuoteBasisErrors.MissingId();

        if (PriceBook == default)
            throw QuoteBasisErrors.MissingPriceBookRef();

        if (!Enum.IsDefined(Status))
            throw QuoteBasisErrors.InvalidStateTransition(Status, "validate");
    }
}
