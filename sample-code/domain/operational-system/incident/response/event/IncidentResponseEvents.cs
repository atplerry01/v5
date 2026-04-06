using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Incident.Response;

public sealed record IncidentDetectedEvent(Guid IncidentId, string IncidentType, string AffectedScope, string AffectedRegion) : DomainEvent;
public sealed record IncidentHaltedEvent(Guid IncidentId, string HaltedBy) : DomainEvent;
public sealed record IncidentInvestigationStartedEvent(Guid IncidentId, string InvestigatorId) : DomainEvent;
public sealed record IncidentResolvedEvent(Guid IncidentId, string Resolution) : DomainEvent;
public sealed record IncidentClosedEvent(Guid IncidentId) : DomainEvent;
