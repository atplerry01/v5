namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.SharedKernel;

public sealed record ReportRejectedEvent(
    Guid ReportId,
    string Reason) : DomainEvent;
