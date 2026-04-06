using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Incident.Response;

/// <summary>
/// Incident response lifecycle: Detected → Halted → Investigating → Resolved → Closed.
/// Coordinates the detection-halt-investigation-resume loop.
/// </summary>
public sealed class IncidentResponseAggregate : AggregateRoot
{
    public string IncidentType { get; private set; } = string.Empty;
    public string AffectedScope { get; private set; } = string.Empty;
    public string AffectedRegion { get; private set; } = string.Empty;
    public IncidentResponseStatus Status { get; private set; } = IncidentResponseStatus.Detected;
    public string? Resolution { get; private set; }

    public static IncidentResponseAggregate Detect(
        Guid id, string incidentType, string affectedScope, string affectedRegion)
    {
        var agg = new IncidentResponseAggregate
        {
            Id = id,
            IncidentType = incidentType,
            AffectedScope = affectedScope,
            AffectedRegion = affectedRegion
        };
        agg.RaiseDomainEvent(new IncidentDetectedEvent(id, incidentType, affectedScope, affectedRegion));
        return agg;
    }

    public void Halt(string haltedBy)
    {
        EnsureInvariant(Status == IncidentResponseStatus.Detected,
            "MustBeDetected", "Can only halt from Detected status.");
        Status = IncidentResponseStatus.Halted;
        RaiseDomainEvent(new IncidentHaltedEvent(Id, haltedBy));
    }

    public void StartInvestigation(string investigatorId)
    {
        EnsureInvariant(Status == IncidentResponseStatus.Halted,
            "MustBeHalted", "Must halt before investigating.");
        Status = IncidentResponseStatus.Investigating;
        RaiseDomainEvent(new IncidentInvestigationStartedEvent(Id, investigatorId));
    }

    public void Resolve(string resolution)
    {
        EnsureInvariant(Status == IncidentResponseStatus.Investigating,
            "MustBeInvestigating", "Must be investigating to resolve.");
        Status = IncidentResponseStatus.Resolved;
        Resolution = resolution;
        RaiseDomainEvent(new IncidentResolvedEvent(Id, resolution));
    }

    public void Close()
    {
        EnsureInvariant(Status == IncidentResponseStatus.Resolved,
            "MustBeResolved", "Must be resolved before closing.");
        Status = IncidentResponseStatus.Closed;
        RaiseDomainEvent(new IncidentClosedEvent(Id));
    }
}
