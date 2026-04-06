namespace Whycespace.Systems.Midstream.Wss.Workflows.Workforce.AssignTask;

/// <summary>
/// Declarative workflow definition for assigning a workforce member to a task.
/// Defines steps and order ONLY — NO execution logic.
/// Execution is handled by T1M step executors via runtime.
/// </summary>
public static class AssignWorkforceToTaskWorkflowDefinition
{
    public const string WorkflowId = "workforce.assign-task";

    public static IReadOnlyList<string> Steps =>
    [
        "identity.validate-eligibility",
        "policy.evaluate",
        "workforce.assign-task",
        "event.emit-assignment"
    ];
}
