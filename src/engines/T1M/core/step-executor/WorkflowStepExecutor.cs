using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Core.StepExecutor;

public sealed class WorkflowStepExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public WorkflowStepExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): forwards the
    // per-step linked CancellationToken built by T1MWorkflowEngine
    // into the step's ExecuteAsync. The token carries both the
    // per-step deadline and the upstream execution / caller cancellation.
    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowStepDefinition stepDefinition,
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var step = (IWorkflowStep)_serviceProvider.GetRequiredService(stepDefinition.StepHandlerType);
        context.CurrentStep = stepDefinition.StepName;

        return await step.ExecuteAsync(context, cancellationToken);
    }
}
