namespace Whycespace.Engines.T2E.Operational.Incident;

public abstract record IncidentCommand;

public sealed record CreateIncidentCommand(
    string AggregateId,
    string IncidentType,
    string Severity,
    string Source,
    Guid AffectedEntityId,
    string Description,
    string? ReferenceDomain = null,
    Guid? ReferenceEntityId = null,
    string? SourceCorrelationId = null
) : IncidentCommand;

public sealed record AssignIncidentCommand(
    string AggregateId,
    Guid AssigneeIdentityId,
    int EscalationLevel = 1
) : IncidentCommand;

public sealed record EscalateIncidentCommand(
    string AggregateId
) : IncidentCommand;

public sealed record ResolveIncidentCommand(
    string AggregateId
) : IncidentCommand;

public sealed record CloseIncidentCommand(
    string AggregateId
) : IncidentCommand;
