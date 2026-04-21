using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.SpendControl;

public static class SpendControlErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("SpendControl has already been initialized.");
}
