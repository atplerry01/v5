using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Whyce.Engines.T1M.StepExecutor;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.WorkflowEngine;

public sealed class T1MWorkflowEngine : IWorkflowEngine
{
    private readonly WorkflowStepExecutor _stepExecutor;

    public T1MWorkflowEngine(WorkflowStepExecutor stepExecutor)
    {
        _stepExecutor = stepExecutor;
    }

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowExecutionContext context)
    {
        var startIndex = context.CurrentStepIndex;
        for (var i = startIndex; i < definition.Steps.Count; i++)
        {
            var stepDefinition = definition.Steps[i];
            context.CurrentStepIndex = i;

            var stepResult = await _stepExecutor.ExecuteAsync(stepDefinition, context);

            if (!stepResult.IsSuccess)
            {
                var error = stepResult.Error ?? $"Step '{stepDefinition.StepName}' failed.";
                if (context.StepObserver is not null)
                {
                    await context.StepObserver.OnWorkflowFailedAsync(context, stepDefinition.StepName, error);
                }

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

            if (context.StepObserver is not null)
            {
                await context.StepObserver.OnStepCompletedAsync(context, i, stepDefinition.StepName);
            }
        }

        if (context.StepObserver is not null)
        {
            await context.StepObserver.OnWorkflowCompletedAsync(context);
        }

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
