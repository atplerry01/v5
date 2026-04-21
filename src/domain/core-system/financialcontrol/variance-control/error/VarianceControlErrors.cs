using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.VarianceControl;

public static class VarianceControlErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("VarianceControl has already been initialized.");
}
