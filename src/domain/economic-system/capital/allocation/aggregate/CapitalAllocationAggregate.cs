using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed class CapitalAllocationAggregate : AggregateRoot
{
    public AllocationId AllocationId { get; private set; }
    public Guid SourceAccountId { get; private set; }
    public TargetId TargetId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public AllocationStatus Status { get; private set; }
    public Timestamp AllocatedAt { get; private set; }

    public void Allocate(
        AllocationId allocationId,
        Guid sourceAccountId,
        TargetId targetId,
        Amount amount,
        Currency currency,
        Timestamp allocatedAt)
    {
        if (amount.Value <= 0) throw AllocationErrors.InvalidAmount();

        RaiseDomainEvent(new AllocationCreatedEvent(
            allocationId, sourceAccountId, targetId, amount, currency, allocatedAt));
    }

    public void Release(Timestamp releasedAt)
    {
        if (Status == AllocationStatus.Completed) throw AllocationErrors.CannotReleaseCompletedAllocation();
        if (Status == AllocationStatus.Released) throw AllocationErrors.AllocationAlreadyReleased();

        RaiseDomainEvent(new AllocationReleasedEvent(
            AllocationId, SourceAccountId, Amount, releasedAt));
    }

    public void Complete(Timestamp completedAt)
    {
        if (Status == AllocationStatus.Released) throw AllocationErrors.CannotCompleteReleasedAllocation();
        if (Status == AllocationStatus.Completed) throw AllocationErrors.AllocationAlreadyCompleted();

        RaiseDomainEvent(new AllocationCompletedEvent(AllocationId, completedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AllocationCreatedEvent e:
                AllocationId = e.AllocationId;
                SourceAccountId = e.SourceAccountId;
                TargetId = e.TargetId;
                Amount = e.Amount;
                Currency = e.Currency;
                Status = AllocationStatus.Pending;
                AllocatedAt = e.AllocatedAt;
                break;

            case AllocationReleasedEvent:
                Status = AllocationStatus.Released;
                break;

            case AllocationCompletedEvent:
                Status = AllocationStatus.Completed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Amount.Value < 0)
            throw AllocationErrors.NegativeAmount();
    }
}
