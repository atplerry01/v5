using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentAssignedEvent(
    Guid IncidentId,
    Guid AssigneeIdentityId,
    int EscalationLevel
) : DomainEvent;
