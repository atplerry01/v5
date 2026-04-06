using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentResolvedEvent(
    Guid IncidentId
) : DomainEvent;
