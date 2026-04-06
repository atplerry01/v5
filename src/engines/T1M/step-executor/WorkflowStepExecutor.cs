using Microsoft.Extensions.DependencyInjection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.StepExecutor;

public sealed class WorkflowStepExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public WorkflowStepExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowStepDefinition stepDefinition,
        WorkflowExecutionContext context)
    {
        var step = (IWorkflowStep)_serviceProvider.GetRequiredService(stepDefinition.StepHandlerType);
        context.CurrentStep = stepDefinition.StepName;

        return await step.ExecuteAsync(context);
    }
}
