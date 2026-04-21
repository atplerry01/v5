using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationReport;

public readonly record struct ReconciliationReportId
{
    public Guid Value { get; }

    public ReconciliationReportId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReconciliationReportId cannot be empty.");
        Value = value;
    }
}
