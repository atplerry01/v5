using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Timeline;

public static class TimelineErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Timeline has already been initialized.");
}
