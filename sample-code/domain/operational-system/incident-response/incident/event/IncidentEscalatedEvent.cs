using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentEscalatedEvent(
    Guid IncidentId,
    string PreviousSeverity,
    string NewSeverity,
    Guid? InvestigatorIdentityId
) : DomainEvent;
