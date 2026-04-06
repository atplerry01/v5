namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.SharedKernel;

public sealed record ReportRevisionRequestedEvent(
    Guid ReportId,
    string Reason) : DomainEvent;
