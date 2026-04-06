namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.SharedKernel;

public sealed record ReportSubmittedEvent(Guid ReportId) : DomainEvent;
