namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

/// <summary>
/// Defines the incident lifecycle workflow — step sequence, conditional and continuous markers.
/// Systems layer: composition only, no execution.
/// </summary>
public static class IncidentWorkflowDefinition
{
    public const string WorkflowId = "INCIDENT_LIFECYCLE_V1";

    public static readonly IReadOnlyList<string> Steps =
    [
        "CreateIncident",
        "AssignIncident",
        "StartProgress",
        "SLACheck",
        "EscalateIncident",
        "ResolveIncident",
        "CloseIncident"
    ];

    public static readonly IReadOnlyList<string> ConditionalSteps =
    [
        "EscalateIncident"
    ];

    public static readonly IReadOnlyList<string> ContinuousSteps =
    [
        "SLACheck"
    ];

    public static bool IsConditional(string stepName) => ConditionalSteps.Contains(stepName);
    public static bool IsContinuous(string stepName) => ContinuousSteps.Contains(stepName);
}
