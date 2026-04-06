namespace Whycespace.Systems.Midstream.Wss.Workflows.Workforce.TriggerIncentive;

/// <summary>
/// Declarative workflow definition for triggering participant incentives.
/// Defines steps and order ONLY — NO execution logic.
/// Execution is handled by T1M step executors via runtime.
/// </summary>
public static class TriggerIncentiveWorkflowDefinition
{
    public const string WorkflowId = "workforce.trigger-incentive";

    public static IReadOnlyList<string> Steps =>
    [
        "incentive.trigger"
    ];
}
