using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.GlobalInvariant;

public static class GlobalInvariantErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("GlobalInvariant has already been initialized.");
}
