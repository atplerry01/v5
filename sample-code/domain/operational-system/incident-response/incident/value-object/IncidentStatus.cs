namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentStatus(string Value)
{
    public static readonly IncidentStatus Created = new("created");
    public static readonly IncidentStatus Assigned = new("assigned");
    public static readonly IncidentStatus InProgress = new("in_progress");
    public static readonly IncidentStatus Escalated = new("escalated");
    public static readonly IncidentStatus Resolved = new("resolved");
    public static readonly IncidentStatus Closed = new("closed");

    // Backward compatibility aliases
    public static readonly IncidentStatus Open = Created;
    public static readonly IncidentStatus Investigating = InProgress;
    public static readonly IncidentStatus Mitigated = Resolved;

    public bool IsTerminal => this == Resolved || this == Closed;
    public bool IsActive => this == Created || this == Assigned || this == InProgress || this == Escalated;
}
