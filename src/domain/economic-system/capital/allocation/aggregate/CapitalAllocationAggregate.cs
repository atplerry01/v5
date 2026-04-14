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

    // ── SPV ownership extension (Phase 2C) ───────────────────────

    public TargetType? TargetType { get; private set; }
    public string? SpvTargetId { get; private set; }
    public decimal? OwnershipPercentage { get; private set; }

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

    // Phase 2C: allocation defines ownership in an SPV. Ownership is
    // declared AFTER the base allocation is created (Allocate has set
    // AllocationId). No vault access, no totals computed here.
    public void AllocateToSpv(string targetId, decimal ownershipPercentage)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("SPV targetId cannot be empty.", nameof(targetId));

        if (ownershipPercentage <= 0m || ownershipPercentage > 100m)
            throw new ArgumentException(
                "OwnershipPercentage must be greater than 0 and less than or equal to 100.",
                nameof(ownershipPercentage));

        RaiseDomainEvent(new CapitalAllocatedToSpvEvent(
            AllocationId.Value.ToString(),
            targetId,
            ownershipPercentage));
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

            case CapitalAllocatedToSpvEvent e:
                TargetType = Allocation.TargetType.SPV;
                SpvTargetId = e.TargetId;
                OwnershipPercentage = e.OwnershipPercentage;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Amount.Value < 0)
            throw AllocationErrors.NegativeAmount();

        if (TargetType == Allocation.TargetType.SPV)
        {
            if (OwnershipPercentage is null || OwnershipPercentage <= 0m || OwnershipPercentage > 100m)
                throw new DomainInvariantViolationException(
                    "Invariant violated: SPV allocation OwnershipPercentage must be in (0, 100].");

            if (string.IsNullOrWhiteSpace(SpvTargetId))
                throw new DomainInvariantViolationException(
                    "Invariant violated: SPV allocation requires a non-empty target id.");
        }
    }
}
