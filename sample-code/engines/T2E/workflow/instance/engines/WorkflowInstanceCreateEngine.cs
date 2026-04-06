using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Workflow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Workflow.Instance;

public sealed class WorkflowInstanceCreateEngine : IEngine<CreateWorkflowInstanceCommand>
{
    private readonly IWorkflowDomainService _workflowDomainService;

    public WorkflowInstanceCreateEngine(IWorkflowDomainService workflowDomainService)
    {
        _workflowDomainService = workflowDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(
        CreateWorkflowInstanceCommand command,
        EngineContext context,
        CancellationToken ct)
    {
        // 1. Validate via T0U decision gate
        var validation = await context.ValidateAsync(command.Id, ct);
        if (!validation.IsValid)
            return EngineResult.Fail(validation.Reason ?? "Validation failed", "WORKFLOW_INSTANCE_CREATE_INVALID");

        // 2. Execute domain logic via domain service
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "workflow",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _workflowDomainService.CreateInstanceAsync(execCtx, command.Id);

        // 3. Return
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
