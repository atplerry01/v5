using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

/// <summary>
/// Time correctness authority. Enforces:
/// - Monotonic timestamp ordering
/// - Non-overlapping scheduling constraints
/// - Sequence ordering guarantees
/// </summary>
public sealed class TemporalAggregate : AggregateRoot
{
    public OrderingGuarantee Guarantee { get; private set; } = OrderingGuarantee.Strict;
    public TemporalSequence Sequence { get; private set; } = null!;
    private readonly List<SchedulingConstraint> _constraints = [];
    public IReadOnlyList<SchedulingConstraint> Constraints => _constraints.AsReadOnly();
    public bool IsSealed { get; private set; }

    public static TemporalAggregate Initialize(Guid id, OrderingGuarantee guarantee, DateTimeOffset startTime)
    {
        var agg = new TemporalAggregate
        {
            Id = id,
            Guarantee = guarantee,
            Sequence = TemporalSequence.Initial(startTime),
            IsSealed = false
        };
        agg.RaiseDomainEvent(new TemporalOrderingInitializedEvent(id, guarantee.Value, startTime));
        return agg;
    }

    public void AdvanceSequence(DateTimeOffset timestamp)
    {
        EnsureNotSealed();
        if (Guarantee.IsStrict && !Sequence.IsMonotonicWith(timestamp))
        {
            RaiseDomainEvent(new OrderingViolationDetectedEvent(Id, Sequence.LastTimestamp, timestamp));
            throw new MonotonicOrderingViolationException(Sequence.LastTimestamp, timestamp);
        }
        Sequence = Sequence.Advance(timestamp);
        RaiseDomainEvent(new SequenceAdvancedEvent(Id, Sequence.SequenceNumber, timestamp));
    }

    public void RegisterConstraint(SchedulingConstraint constraint)
    {
        EnsureNotSealed();
        foreach (var existing in _constraints)
        {
            EnsureInvariant(
                !existing.Overlaps(constraint),
                "NoOverlappingSchedules",
                $"New constraint [{constraint.NotBefore} - {constraint.NotAfter}] overlaps with [{existing.NotBefore} - {existing.NotAfter}]");
        }
        _constraints.Add(constraint);
        RaiseDomainEvent(new SchedulingConstraintRegisteredEvent(
            Id, constraint.NotBefore, constraint.NotAfter, constraint.RequiresMonotonic));
    }

    public void Seal()
    {
        EnsureNotSealed();
        IsSealed = true;
        RaiseDomainEvent(new TemporalOrderingSealedEvent(Id, Sequence.SequenceNumber));
    }

    private void EnsureNotSealed()
    {
        if (IsSealed)
            throw new TemporalOrderingSealedException();
    }
}
