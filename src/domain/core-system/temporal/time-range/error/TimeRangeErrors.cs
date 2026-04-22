using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeRange;

public static class TimeRangeErrors
{
    public static DomainException StartMustNotBeDefault() =>
        new DomainInvariantViolationException("TimeRange start must not be the default DateTimeOffset value.");

    public static DomainException EndMustNotBeDefault() =>
        new DomainInvariantViolationException("TimeRange end must not be the default DateTimeOffset value.");

    public static DomainException EndMustFollowStart(DateTimeOffset start, DateTimeOffset end) =>
        new DomainInvariantViolationException(
            $"TimeRange end ({end:O}) must be strictly after start ({start:O}). A zero-duration range is not permitted.");
}
