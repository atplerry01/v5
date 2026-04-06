namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.SharedKernel;

public sealed record ReportAcceptedEvent(Guid ReportId) : DomainEvent;
