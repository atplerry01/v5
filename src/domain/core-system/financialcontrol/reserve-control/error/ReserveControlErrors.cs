using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.ReserveControl;

public static class ReserveControlErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ReserveControl has already been initialized.");
}
