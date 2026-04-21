using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed class FulfillmentInstructionAggregate : AggregateRoot
{
    public FulfillmentInstructionId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public LineItemRef? LineItem { get; private set; }
    public FulfillmentDirective Directive { get; private set; }
    public FulfillmentInstructionStatus Status { get; private set; }

    public static FulfillmentInstructionAggregate Create(
        FulfillmentInstructionId id,
        OrderRef order,
        FulfillmentDirective directive,
        LineItemRef? lineItem = null)
    {
        var aggregate = new FulfillmentInstructionAggregate();
        if (aggregate.Version >= 0)
            throw FulfillmentInstructionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new FulfillmentInstructionCreatedEvent(id, order, lineItem, directive));
        return aggregate;
    }

    public void Issue(DateTimeOffset issuedAt)
    {
        var specification = new CanIssueSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FulfillmentInstructionErrors.InvalidStateTransition(Status, nameof(Issue));

        RaiseDomainEvent(new FulfillmentInstructionIssuedEvent(Id, issuedAt));
    }

    public void Complete(DateTimeOffset completedAt)
    {
        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FulfillmentInstructionErrors.InvalidStateTransition(Status, nameof(Complete));

        RaiseDomainEvent(new FulfillmentInstructionCompletedEvent(Id, completedAt));
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FulfillmentInstructionErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new FulfillmentInstructionRevokedEvent(Id, revokedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case FulfillmentInstructionCreatedEvent e:
                Id = e.FulfillmentInstructionId;
                Order = e.Order;
                LineItem = e.LineItem;
                Directive = e.Directive;
                Status = FulfillmentInstructionStatus.Draft;
                break;
            case FulfillmentInstructionIssuedEvent:
                Status = FulfillmentInstructionStatus.Issued;
                break;
            case FulfillmentInstructionCompletedEvent:
                Status = FulfillmentInstructionStatus.Completed;
                break;
            case FulfillmentInstructionRevokedEvent:
                Status = FulfillmentInstructionStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw FulfillmentInstructionErrors.MissingId();

        if (Order == default)
            throw FulfillmentInstructionErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw FulfillmentInstructionErrors.InvalidStateTransition(Status, "validate");
    }
}
