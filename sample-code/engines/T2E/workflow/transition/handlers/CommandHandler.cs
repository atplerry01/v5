using Whycespace.Shared.Contracts.Domain.Workflow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Workflow.Transition;

public sealed class WorkflowTransitionCommandHandler
{
    private readonly WorkflowTransitionCreateEngine _createEngine;

    public WorkflowTransitionCommandHandler(IWorkflowDomainService workflowDomainService)
    {
        _createEngine = new WorkflowTransitionCreateEngine(workflowDomainService);
    }

    public Task<EngineResult> HandleAsync(
        WorkflowTransitionCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateWorkflowTransitionCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
