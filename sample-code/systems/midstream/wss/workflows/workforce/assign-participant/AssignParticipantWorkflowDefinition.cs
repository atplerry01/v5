namespace Whycespace.Systems.Midstream.Wss.Workflows.Workforce.AssignParticipant;

/// <summary>
/// Declarative workflow definition for assigning a participant to a workforce.
/// Defines steps and order ONLY — NO execution logic.
/// Execution is handled by T1M step executors via runtime.
/// </summary>
public static class AssignParticipantWorkflowDefinition
{
    public const string WorkflowId = "workforce.assign-participant";

    public static IReadOnlyList<string> Steps =>
    [
        "workforce.assign-participant"
    ];
}
