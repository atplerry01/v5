using Whycespace.Shared.Contracts.Domain.Workflow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Workflow.Definition;

public sealed class WorkflowDefinitionCommandHandler
{
    private readonly WorkflowDefinitionCreateEngine _createEngine;

    public WorkflowDefinitionCommandHandler(IWorkflowDomainService workflowDomainService)
    {
        _createEngine = new WorkflowDefinitionCreateEngine(workflowDomainService);
    }

    public Task<EngineResult> HandleAsync(
        WorkflowDefinitionCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateWorkflowDefinitionCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
