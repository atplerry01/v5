using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationReport;

public sealed record ReconciliationReportCreatedEvent(Guid ReportId, string ReportPeriod, string GeneratedBy) : DomainEvent;
public sealed record ReconciliationReportFinalizedEvent(Guid ReportId) : DomainEvent;
