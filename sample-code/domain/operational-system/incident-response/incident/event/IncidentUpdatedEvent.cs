using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentUpdatedEvent(
    Guid IncidentId,
    string Description
) : DomainEvent;
