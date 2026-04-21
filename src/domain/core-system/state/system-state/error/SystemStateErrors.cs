using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.SystemState;

public static class SystemStateErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("SystemState has already been initialized.");
}
