using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationReport;

public sealed class ReconciliationReportAggregate : AggregateRoot
{
    public string ReportPeriod { get; private set; } = string.Empty;
    public string GeneratedBy { get; private set; } = string.Empty;
    public bool IsFinalized { get; private set; }

    public static ReconciliationReportAggregate Create(Guid id, string reportPeriod, string generatedBy)
    {
        var agg = new ReconciliationReportAggregate
        {
            Id = id,
            ReportPeriod = reportPeriod,
            GeneratedBy = generatedBy,
            IsFinalized = false
        };
        agg.RaiseDomainEvent(new ReconciliationReportCreatedEvent(id, reportPeriod, generatedBy));
        return agg;
    }

    public void FinalizeReport()
    {
        IsFinalized = true;
        RaiseDomainEvent(new ReconciliationReportFinalizedEvent(Id));
    }
}
