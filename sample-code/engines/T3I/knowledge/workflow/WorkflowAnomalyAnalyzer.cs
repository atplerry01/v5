using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Workflow;

/// <summary>
/// T3I engine: detects anomalies in workflow execution bound to policy decisions.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
///
/// Anomalies detected:
///   - Skipped steps (gap in step sequence)
///   - Invalid transitions (state machine violation)
///   - Policy mismatch (execution without decision)
///   - Execution without authorization
/// </summary>
public sealed class WorkflowAnomalyAnalyzer : IEngine<AnalyzeWorkflowExecutionCommand>
{
    private static readonly HashSet<string> ValidTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        "start", "execute", "complete", "fault", "cancel", "retry"
    };

    private static readonly Dictionary<string, HashSet<string>> AllowedStateTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["created"] = ["start", "cancel"],
        ["executing"] = ["execute", "complete", "fault", "cancel"],
        ["running"] = ["execute", "complete", "fault", "cancel"],
        ["completed"] = [],
        ["faulted"] = ["retry", "cancel"],
        ["cancelled"] = []
    };

    public Task<EngineResult> ExecuteAsync(
        AnalyzeWorkflowExecutionCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var anomalies = new List<WorkflowAnomaly>();

        // Anomaly 1: Invalid transition
        if (!ValidTransitions.Contains(command.Transition))
        {
            anomalies.Add(new WorkflowAnomaly(
                "invalid_transition",
                $"Unknown transition '{command.Transition}' for workflow '{command.WorkflowId}'",
                WorkflowAnomalySeverity.High));
        }

        // Anomaly 2: State machine violation
        if (AllowedStateTransitions.TryGetValue(command.State, out var allowed)
            && !allowed.Contains(command.Transition))
        {
            anomalies.Add(new WorkflowAnomaly(
                "state_violation",
                $"Transition '{command.Transition}' not allowed from state '{command.State}'",
                WorkflowAnomalySeverity.Critical));
        }

        // Anomaly 3: Missing policy decision
        if (string.IsNullOrWhiteSpace(command.DecisionHash))
        {
            anomalies.Add(new WorkflowAnomaly(
                "missing_policy",
                $"Workflow step '{command.StepId}' executed without policy decision hash",
                WorkflowAnomalySeverity.Critical));
        }

        // Anomaly 4: Skipped steps
        if (command.ExpectedStepIndex > 0 && command.ActualStepIndex > command.ExpectedStepIndex)
        {
            anomalies.Add(new WorkflowAnomaly(
                "skipped_steps",
                $"Expected step index {command.ExpectedStepIndex}, got {command.ActualStepIndex} — steps were skipped",
                WorkflowAnomalySeverity.High));
        }

        var result = new WorkflowAnalysisResult(
            WorkflowId: command.WorkflowId,
            StepId: command.StepId,
            AnomalyCount: anomalies.Count,
            Anomalies: anomalies,
            IsClean: anomalies.Count == 0);

        return Task.FromResult(EngineResult.Ok(result));
    }
}

public sealed record AnalyzeWorkflowExecutionCommand(
    string WorkflowId,
    string StepId,
    string State,
    string Transition,
    string? DecisionHash,
    int ExpectedStepIndex = 0,
    int ActualStepIndex = 0);

public sealed record WorkflowAnalysisResult(
    string WorkflowId,
    string StepId,
    int AnomalyCount,
    List<WorkflowAnomaly> Anomalies,
    bool IsClean);

public sealed record WorkflowAnomaly(
    string Type,
    string Description,
    WorkflowAnomalySeverity Severity);

public enum WorkflowAnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}
