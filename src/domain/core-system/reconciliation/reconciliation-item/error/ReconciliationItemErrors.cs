using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationItem;

public static class ReconciliationItemErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ReconciliationItem has already been initialized.");
}
