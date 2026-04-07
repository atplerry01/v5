using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Whyce.Engines.T1M.Lifecycle;
using Whyce.Engines.T1M.StepExecutor;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.WorkflowEngine;

/// <summary>
/// T1M workflow orchestrator. Executes a workflow definition step-by-step and
/// emits domain lifecycle events through <see cref="WorkflowLifecycleEventFactory"/>
/// into the <see cref="IDomainEventSink"/> on the execution context.
///
/// The engine NEVER mutates aggregate state directly (engine.guard rule 3).
/// Lifecycle transitions are produced as event records by the factory; the
/// runtime drains them through persist → chain → outbox.
/// </summary>
public sealed class T1MWorkflowEngine : IWorkflowEngine
{
    private readonly WorkflowStepExecutor _stepExecutor;
    private readonly WorkflowLifecycleEventFactory _lifecycleFactory;

    public T1MWorkflowEngine(
        WorkflowStepExecutor stepExecutor,
        WorkflowLifecycleEventFactory lifecycleFactory)
    {
        _stepExecutor = stepExecutor;
        _lifecycleFactory = lifecycleFactory;
    }

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowExecutionContext context)
    {
        var startIndex = context.CurrentStepIndex;

        if (startIndex == 0)
        {
            context.EmitEvent(_lifecycleFactory.Started(context.WorkflowId, context.WorkflowName, context.Payload));
        }

        for (var i = startIndex; i < definition.Steps.Count; i++)
        {
            var stepDefinition = definition.Steps[i];
            context.CurrentStepIndex = i;

            var stepResult = await _stepExecutor.ExecuteAsync(stepDefinition, context);

            if (!stepResult.IsSuccess)
            {
                var error = stepResult.Error ?? $"Step '{stepDefinition.StepName}' failed.";
                context.EmitEvent(_lifecycleFactory.Failed(context.WorkflowId, stepDefinition.StepName, error));
                return WorkflowExecutionResult.Failure(stepDefinition.StepName, error);
            }

            context.StepOutputs[stepDefinition.StepName] = stepResult.Output;

            if (stepResult.Events.Count > 0)
            {
                context.AccumulatedEvents.AddRange(stepResult.Events);
            }

            context.ExecutionHash = ComputeStepHash(
                context.ExecutionHash,
                stepDefinition.StepId,
                stepResult.Output);

            context.EmitEvent(_lifecycleFactory.StepCompleted(
                context.WorkflowId, i, stepDefinition.StepName, context.ExecutionHash, stepResult.Output));
        }

        context.EmitEvent(_lifecycleFactory.Completed(context.WorkflowId, context.ExecutionHash));

        return WorkflowExecutionResult.Success(
            context.WorkflowOutput,
            context.AccumulatedEvents.AsReadOnly());
    }

    private static string ComputeStepHash(string previousHash, string stepId, object? output)
    {
        var outputHash = output is not null
            ? JsonSerializer.Serialize(output)
            : string.Empty;

        var input = $"{previousHash}{stepId}{outputHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
