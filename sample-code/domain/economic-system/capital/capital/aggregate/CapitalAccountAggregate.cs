using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public class CapitalAccountAggregate : AggregateRoot
{
    private readonly List<CapitalContribution> _contributions = [];
    private readonly List<CapitalAllocation> _allocations = [];
    private readonly List<CapitalReservation> _reservations = [];
    private readonly List<CapitalCommitment> _commitments = [];
    private readonly List<CapitalAdjustment> _adjustments = [];
    private readonly List<CapitalUtilization> _utilizations = [];
    private readonly List<CapitalLock> _locks = [];
    private readonly List<CapitalRelease> _releases = [];
    private readonly List<CapitalDistribution> _distributions = [];

    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new CapitalAccountCreatedEvent(id));
    }

    public void Commit(Guid capitalAccountId, decimal amount, string currencyCode)
    {
        EnsureInvariant(amount > 0, "PositiveAmount", "Commitment amount must be positive.");
        var lockId = EventId.Deterministic(capitalAccountId, nameof(CapitalLockedEvent), 1, "commit").Value;
        RaiseDomainEvent(new CapitalLockedEvent(capitalAccountId, lockId));
    }

    public void Allocate(Guid capitalAccountId, string allocationTarget, decimal amount)
    {
        EnsureInvariant(amount > 0, "PositiveAmount", "Allocation amount must be positive.");
        Guard.AgainstEmpty(allocationTarget);
        var allocationId = EventId.Deterministic(capitalAccountId, nameof(CapitalAllocatedEvent), 1, allocationTarget).Value;
        RaiseDomainEvent(new CapitalAllocatedEvent(capitalAccountId, allocationId));
    }
}
