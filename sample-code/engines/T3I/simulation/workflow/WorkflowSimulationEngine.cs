using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Simulation;

/// <summary>
/// T3I engine: simulates workflow step transitions and completion paths.
/// Predicts whether a workflow will complete, fault, or block.
/// Stateless, deterministic. NEVER writes to chain or mutates state.
/// </summary>
public sealed class WorkflowSimulationEngine : IEngine<SimulateWorkflowPathCommand>
{
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["created"] = ["start", "cancel"],
        ["executing"] = ["execute", "complete", "fault", "cancel"],
        ["running"] = ["execute", "complete", "fault", "cancel"],
        ["completed"] = [],
        ["faulted"] = ["retry", "cancel"],
        ["cancelled"] = []
    };

    public Task<EngineResult> ExecuteAsync(
        SimulateWorkflowPathCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var transitionAllowed = AllowedTransitions.TryGetValue(command.CurrentState, out var allowed)
            && allowed.Contains(command.Transition);

        var hasBlockedPath = !transitionAllowed || !command.PolicyAllowed;

        var predictedNextState = command.Transition switch
        {
            "start" or "execute" => "executing",
            "complete" => "completed",
            "fault" => "faulted",
            "cancel" => "cancelled",
            "retry" => "executing",
            _ => command.CurrentState
        };

        var completionLikelihood = hasBlockedPath ? 0.0
            : predictedNextState == "completed" ? 1.0
            : predictedNextState == "executing" ? 0.7
            : 0.3;

        var result = new WorkflowSimulationResult(
            WorkflowId: command.WorkflowId,
            StepId: command.StepId,
            CurrentState: command.CurrentState,
            PredictedNextState: transitionAllowed ? predictedNextState : command.CurrentState,
            TransitionAllowed: transitionAllowed,
            HasBlockedPath: hasBlockedPath,
            CompletionLikelihood: completionLikelihood,
            BlockReason: !transitionAllowed
                ? $"Transition '{command.Transition}' not allowed from state '{command.CurrentState}'"
                : !command.PolicyAllowed
                    ? "Policy denied the transition"
                    : null);

        return Task.FromResult(EngineResult.Ok(result));
    }
}

public sealed record SimulateWorkflowPathCommand(
    string WorkflowId,
    string StepId,
    string CurrentState,
    string Transition,
    bool PolicyAllowed);

public sealed record WorkflowSimulationResult(
    string WorkflowId,
    string StepId,
    string CurrentState,
    string PredictedNextState,
    bool TransitionAllowed,
    bool HasBlockedPath,
    double CompletionLikelihood,
    string? BlockReason);
