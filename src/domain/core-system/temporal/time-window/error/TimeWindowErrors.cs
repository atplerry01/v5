using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

public static class TimeWindowErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("TimeWindow has already been initialized.");
}
