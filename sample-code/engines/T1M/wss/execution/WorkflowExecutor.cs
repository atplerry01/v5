namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Stateless workflow executor — executes a sequence of steps resolved from the registry.
/// MUST NOT contain workflow definitions or domain logic.
/// Delegates engine invocation to the dispatcher.
/// </summary>
public sealed class WorkflowExecutor
{
    private readonly StepExecutor _stepExecutor;

    public WorkflowExecutor(StepExecutor stepExecutor)
    {
        _stepExecutor = stepExecutor ?? throw new ArgumentNullException(nameof(stepExecutor));
    }

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        string workflowId,
        IReadOnlyList<string> steps,
        WorkflowExecutionContext context,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(context);

        var results = new List<StepExecutionResult>();

        foreach (var stepName in steps)
        {
            var result = await _stepExecutor.ExecuteAsync(stepName, context, ct);
            results.Add(result);

            if (!result.Success)
                break;
        }

        var allSucceeded = results.Count == steps.Count && results.All(r => r.Success);

        return new WorkflowExecutionResult(
            workflowId,
            allSucceeded,
            results.AsReadOnly());
    }
}

public sealed record WorkflowExecutionResult(
    string WorkflowId,
    bool Success,
    IReadOnlyList<StepExecutionResult> StepResults);
