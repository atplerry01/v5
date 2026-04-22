using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

public static class TimeWindowErrors
{
    public static DomainException StartMustNotBeDefault() =>
        new DomainInvariantViolationException("TimeWindow start must not be the default DateTimeOffset value.");

    public static DomainException EndMustFollowStart(DateTimeOffset start, DateTimeOffset end) =>
        new DomainInvariantViolationException(
            $"TimeWindow end ({end:O}) must be strictly after start ({start:O}).");

    public static DomainException CannotConvertOpenWindowToTimeRange() =>
        new DomainInvariantViolationException(
            "Cannot convert an open-ended TimeWindow to a TimeRange. Close the window with an End value first.");
}
