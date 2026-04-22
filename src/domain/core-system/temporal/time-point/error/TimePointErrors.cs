using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimePoint;

public static class TimePointErrors
{
    public static DomainException TimestampMustNotBeDefault() =>
        new DomainInvariantViolationException("TimePoint timestamp must not be the default DateTimeOffset value.");
}
