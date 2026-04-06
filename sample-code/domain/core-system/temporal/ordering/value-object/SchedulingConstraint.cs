namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

/// <summary>
/// Constraint for scheduling: no overlapping windows and monotonic timestamp enforcement.
/// </summary>
public sealed record SchedulingConstraint
{
    public DateTimeOffset NotBefore { get; }
    public DateTimeOffset NotAfter { get; }
    public bool RequiresMonotonic { get; }

    private SchedulingConstraint(DateTimeOffset notBefore, DateTimeOffset notAfter, bool requiresMonotonic)
    {
        NotBefore = notBefore;
        NotAfter = notAfter;
        RequiresMonotonic = requiresMonotonic;
    }

    public static SchedulingConstraint Create(DateTimeOffset notBefore, DateTimeOffset notAfter, bool requiresMonotonic = true) =>
        notAfter <= notBefore
            ? throw new ArgumentException("NotAfter must be after NotBefore.")
            : new(notBefore, notAfter, requiresMonotonic);

    public bool Overlaps(SchedulingConstraint other) =>
        NotBefore < other.NotAfter && other.NotBefore < NotAfter;

    public bool Contains(DateTimeOffset timestamp) =>
        timestamp >= NotBefore && timestamp <= NotAfter;
}
