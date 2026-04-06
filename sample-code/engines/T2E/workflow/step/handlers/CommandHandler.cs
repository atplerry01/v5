using Whycespace.Shared.Contracts.Domain.Workflow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Workflow.Step;

public sealed class WorkflowStepCommandHandler
{
    private readonly WorkflowStepCreateEngine _createEngine;

    public WorkflowStepCommandHandler(IWorkflowDomainService workflowDomainService)
    {
        _createEngine = new WorkflowStepCreateEngine(workflowDomainService);
    }

    public Task<EngineResult> HandleAsync(
        WorkflowStepCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateWorkflowStepCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
