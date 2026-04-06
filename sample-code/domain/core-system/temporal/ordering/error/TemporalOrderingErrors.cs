using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

public sealed class MonotonicOrderingViolationException : DomainException
{
    public MonotonicOrderingViolationException(DateTimeOffset expected, DateTimeOffset actual)
        : base("MONOTONIC_ORDERING_VIOLATION", $"Expected timestamp after {expected}, but received {actual}.") { }
}

public sealed class OverlappingScheduleException : DomainException
{
    public OverlappingScheduleException(DateTimeOffset existingStart, DateTimeOffset existingEnd)
        : base("OVERLAPPING_SCHEDULE", $"New constraint overlaps with existing window [{existingStart} - {existingEnd}].") { }
}

public sealed class TemporalOrderingSealedException : DomainException
{
    public TemporalOrderingSealedException()
        : base("TEMPORAL_ORDERING_SEALED", "Temporal ordering is sealed and cannot accept further operations.") { }
}
