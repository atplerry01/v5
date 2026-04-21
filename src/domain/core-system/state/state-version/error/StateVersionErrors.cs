using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateVersion;

public static class StateVersionErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("StateVersion has already been initialized.");
}
