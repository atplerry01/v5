namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentReportedEvent(IncidentId IncidentId, IncidentDescriptor Descriptor);

public sealed record IncidentInvestigationStartedEvent(IncidentId IncidentId);

public sealed record IncidentResolvedEvent(IncidentId IncidentId);

public sealed record IncidentClosedEvent(IncidentId IncidentId);
