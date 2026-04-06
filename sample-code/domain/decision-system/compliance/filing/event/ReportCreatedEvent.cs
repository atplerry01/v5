namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.SharedKernel;

public sealed record ReportCreatedEvent(
    Guid ReportId,
    Guid RegulationId,
    Guid JurisdictionId,
    string Title,
    string ReportingPeriod) : DomainEvent;
