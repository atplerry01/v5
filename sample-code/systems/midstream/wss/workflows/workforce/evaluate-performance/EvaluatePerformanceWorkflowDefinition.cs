namespace Whycespace.Systems.Midstream.Wss.Workflows.Workforce.EvaluatePerformance;

/// <summary>
/// Declarative workflow definition for evaluating participant performance.
/// Defines steps and order ONLY — NO execution logic.
/// Execution is handled by T1M step executors via runtime.
/// </summary>
public static class EvaluatePerformanceWorkflowDefinition
{
    public const string WorkflowId = "workforce.evaluate-performance";

    public static IReadOnlyList<string> Steps =>
    [
        "performance.evaluate"
    ];
}
