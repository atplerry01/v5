namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

/// <summary>
/// Contract for incident workflow steps.
/// Systems layer defines the step interface; T1M executes via this contract.
/// </summary>
public interface IIncidentWorkflowStep
{
    Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default);
}

public abstract record IncidentStepCommand(Guid IncidentId);

public sealed record CreateIncidentStepCommand(
    Guid IncidentId,
    string IncidentType,
    string Severity,
    string Source,
    Guid AffectedEntityId,
    string Description,
    string? ReferenceDomain = null,
    Guid? ReferenceEntityId = null
) : IncidentStepCommand(IncidentId);

public sealed record AssignIncidentStepCommand(
    Guid IncidentId,
    Guid AssigneeIdentityId,
    int EscalationLevel = 1
) : IncidentStepCommand(IncidentId);

public sealed record StartProgressStepCommand(
    Guid IncidentId
) : IncidentStepCommand(IncidentId);

public sealed record EscalateIncidentStepCommand(
    Guid IncidentId
) : IncidentStepCommand(IncidentId);

public sealed record ResolveIncidentStepCommand(
    Guid IncidentId
) : IncidentStepCommand(IncidentId);

public sealed record CloseIncidentStepCommand(
    Guid IncidentId
) : IncidentStepCommand(IncidentId);

public sealed record CheckSLAStepCommand(
    Guid IncidentId
) : IncidentStepCommand(IncidentId);

public sealed record IncidentStepResult(
    Guid IncidentId,
    string StepName,
    bool Success,
    string? FailureReason = null)
{
    public static IncidentStepResult Ok(Guid incidentId, string stepName)
        => new(incidentId, stepName, true);

    public static IncidentStepResult Fail(Guid incidentId, string stepName, string reason)
        => new(incidentId, stepName, false, reason);
}
