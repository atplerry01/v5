namespace Whycespace.Projections.Core.Reconciliation.ReconciliationReport;

public interface IReconciliationReportViewRepository
{
    Task SaveAsync(ReconciliationReportReadModel model, CancellationToken ct = default);
    Task<ReconciliationReportReadModel?> GetAsync(string id, CancellationToken ct = default);
}
