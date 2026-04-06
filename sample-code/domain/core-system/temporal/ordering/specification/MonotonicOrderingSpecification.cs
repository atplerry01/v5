namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

/// <summary>
/// Specification: a timestamp must be strictly after the last observed timestamp.
/// Pure predicate — no side effects.
/// </summary>
public sealed class MonotonicOrderingSpecification
{
    public bool IsSatisfiedBy(TemporalSequence sequence, DateTimeOffset next) =>
        sequence.IsMonotonicWith(next);
}
