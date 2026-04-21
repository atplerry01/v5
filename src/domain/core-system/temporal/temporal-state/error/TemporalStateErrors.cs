using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TemporalState;

public static class TemporalStateErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("TemporalState has already been initialized.");
}
