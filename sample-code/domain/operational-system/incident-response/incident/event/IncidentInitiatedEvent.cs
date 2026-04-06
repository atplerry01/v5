namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

/// <summary>
/// Topic: whyce.operational.global.incident-response.initiated
/// Command: IncidentInitiateCommand
/// </summary>
public sealed record IncidentInitiatedEvent(
    Guid IncidentId,
    string Category,
    Guid AffectedEntityId) : DomainEvent;
