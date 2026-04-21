using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationRun;

public static class ReconciliationRunErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ReconciliationRun has already been initialized.");
}
