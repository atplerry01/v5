using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed class FulfillmentInstructionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public FulfillmentInstructionId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public LineItemRef? LineItem { get; private set; }
    public FulfillmentDirective Directive { get; private set; }
    public FulfillmentInstructionStatus Status { get; private set; }
    public int Version { get; private set; }

    private FulfillmentInstructionAggregate() { }

    public static FulfillmentInstructionAggregate Create(
        FulfillmentInstructionId id,
        OrderRef order,
        FulfillmentDirective directive,
        LineItemRef? lineItem = null)
    {
        var aggregate = new FulfillmentInstructionAggregate();

        var @event = new FulfillmentInstructionCreatedEvent(id, order, lineItem, directive);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Issue(DateTimeOffset issuedAt)
    {
        var specification = new CanIssueSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FulfillmentInstructionErrors.InvalidStateTransition(Status, nameof(Issue));

        var @event = new FulfillmentInstructionIssuedEvent(Id, issuedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete(DateTimeOffset completedAt)
    {
        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FulfillmentInstructionErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new FulfillmentInstructionCompletedEvent(Id, completedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FulfillmentInstructionErrors.AlreadyTerminal(Id, Status);

        var @event = new FulfillmentInstructionRevokedEvent(Id, revokedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(FulfillmentInstructionCreatedEvent @event)
    {
        Id = @event.FulfillmentInstructionId;
        Order = @event.Order;
        LineItem = @event.LineItem;
        Directive = @event.Directive;
        Status = FulfillmentInstructionStatus.Draft;
        Version++;
    }

    private void Apply(FulfillmentInstructionIssuedEvent @event)
    {
        Status = FulfillmentInstructionStatus.Issued;
        Version++;
    }

    private void Apply(FulfillmentInstructionCompletedEvent @event)
    {
        Status = FulfillmentInstructionStatus.Completed;
        Version++;
    }

    private void Apply(FulfillmentInstructionRevokedEvent @event)
    {
        Status = FulfillmentInstructionStatus.Revoked;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw FulfillmentInstructionErrors.MissingId();

        if (Order == default)
            throw FulfillmentInstructionErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw FulfillmentInstructionErrors.InvalidStateTransition(Status, "validate");
    }
}
