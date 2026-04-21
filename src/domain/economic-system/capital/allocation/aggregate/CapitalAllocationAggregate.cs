using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed class CapitalAllocationAggregate : AggregateRoot
{
    public AllocationId AllocationId { get; private set; }
    public AccountId SourceAccountId { get; private set; }
    public TargetId TargetId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public AllocationStatus Status { get; private set; }
    public Timestamp AllocatedAt { get; private set; }

    // ── SPV ownership extension (Phase 2C) ───────────────────────

    public TargetType? TargetType { get; private set; }
    public SpvRef? TargetSpv { get; private set; }
    public decimal? OwnershipPercentage { get; private set; }

    public void Allocate(
        AllocationId allocationId,
        AccountId sourceAccountId,
        TargetId targetId,
        Amount amount,
        Currency currency,
        Timestamp allocatedAt)
    {
        if (amount.Value <= 0) throw AllocationErrors.InvalidAmount();

        RaiseDomainEvent(new AllocationCreatedEvent(
            allocationId, sourceAccountId.Value, targetId, amount, currency, allocatedAt));
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public void Allocate(
        AllocationId allocationId,
        Guid sourceAccountId,
        TargetId targetId,
        Amount amount,
        Currency currency,
        Timestamp allocatedAt)
        => Allocate(allocationId, new AccountId(sourceAccountId), targetId, amount, currency, allocatedAt);

    public void Release(Timestamp releasedAt)
    {
        if (Status == AllocationStatus.Completed) throw AllocationErrors.CannotReleaseCompletedAllocation();
        if (Status == AllocationStatus.Released) throw AllocationErrors.AllocationAlreadyReleased();

        RaiseDomainEvent(new AllocationReleasedEvent(
            AllocationId, SourceAccountId.Value, Amount, releasedAt));
    }

    public void Complete(Timestamp completedAt)
    {
        if (Status == AllocationStatus.Released) throw AllocationErrors.CannotCompleteReleasedAllocation();
        if (Status == AllocationStatus.Completed) throw AllocationErrors.AllocationAlreadyCompleted();

        RaiseDomainEvent(new AllocationCompletedEvent(AllocationId, completedAt));
    }

    public void AllocateToSpv(SpvRef targetSpv, decimal ownershipPercentage)
    {
        if (targetSpv == default)
            throw AllocationErrors.InvalidSpvTargetId();

        if (ownershipPercentage <= 0m || ownershipPercentage > 100m)
            throw AllocationErrors.InvalidOwnershipPercentage();

        RaiseDomainEvent(new CapitalAllocatedToSpvEvent(
            AllocationId.Value.ToString(),
            targetSpv.Value.ToString(),
            ownershipPercentage));
    }

    public void AllocateToSpv(string targetId, decimal ownershipPercentage)
    {
        if (string.IsNullOrWhiteSpace(targetId) ||
            !Guid.TryParse(targetId, out var parsed) ||
            parsed == Guid.Empty)
            throw AllocationErrors.InvalidSpvTargetId();

        AllocateToSpv(new SpvRef(parsed), ownershipPercentage);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AllocationCreatedEvent e:
                AllocationId = e.AllocationId;
                SourceAccountId = new AccountId(e.SourceAccountId);
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
                TargetSpv = e.TargetSpv;
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

            if (TargetSpv is null)
                throw new DomainInvariantViolationException(
                    "Invariant violated: SPV allocation requires a valid SpvRef target.");
        }
    }
}
