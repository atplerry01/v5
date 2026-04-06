using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentClosedEvent(
    Guid IncidentId
) : DomainEvent;
