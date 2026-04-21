using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationException;

public static class ReconciliationExceptionErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ReconciliationException has already been initialized.");
}
