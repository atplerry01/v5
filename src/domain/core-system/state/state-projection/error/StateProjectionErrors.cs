using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateProjection;

public static class StateProjectionErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("StateProjection has already been initialized.");
}
