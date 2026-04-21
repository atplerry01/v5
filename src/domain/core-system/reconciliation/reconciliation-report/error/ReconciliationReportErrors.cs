using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationReport;

public static class ReconciliationReportErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ReconciliationReport has already been initialized.");
}
