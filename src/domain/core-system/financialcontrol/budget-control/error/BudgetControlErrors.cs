using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.BudgetControl;

public static class BudgetControlErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("BudgetControl has already been initialized.");
}
