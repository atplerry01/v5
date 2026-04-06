using Whycespace.Shared.Contracts.Domain.Workflow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Workflow.Instance;

public sealed class WorkflowInstanceCommandHandler
{
    private readonly WorkflowInstanceCreateEngine _createEngine;

    public WorkflowInstanceCommandHandler(IWorkflowDomainService workflowDomainService)
    {
        _createEngine = new WorkflowInstanceCreateEngine(workflowDomainService);
    }

    public Task<EngineResult> HandleAsync(
        WorkflowInstanceCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateWorkflowInstanceCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
