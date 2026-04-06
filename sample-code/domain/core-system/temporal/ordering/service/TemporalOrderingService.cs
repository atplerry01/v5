namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

/// <summary>
/// Domain service for temporal ordering evaluation.
/// Stateless — all data passed as parameters.
/// </summary>
public sealed class TemporalOrderingService
{
    public bool CanAdvance(TemporalAggregate ordering, DateTimeOffset timestamp) =>
        !ordering.IsSealed && ordering.Sequence.IsMonotonicWith(timestamp);

    public bool HasOverlap(TemporalAggregate ordering, SchedulingConstraint constraint) =>
        ordering.Constraints.Any(c => c.Overlaps(constraint));
}
