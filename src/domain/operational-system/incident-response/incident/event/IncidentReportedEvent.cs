namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentReportedEvent(IncidentId IncidentId, IncidentDescriptor Descriptor);
