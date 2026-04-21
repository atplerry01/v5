using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Clock;

public static class ClockErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Clock has already been initialized.");
}
