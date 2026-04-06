using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentCreatedEvent(
    Guid IncidentId,
    string IncidentType,
    string Severity,
    string Priority,
    string Source,
    Guid AffectedEntityId,
    string Description,
    string? ReferenceDomain,
    Guid? ReferenceEntityId,
    string? SourceCorrelationId = null
) : DomainEvent;
