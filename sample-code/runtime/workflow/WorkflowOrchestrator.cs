using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Runtime.Workflow;

public sealed class WorkflowOrchestrator
{
    private readonly WorkflowResolver _workflowResolver;
    private readonly WorkflowExecutor _executor;
    private readonly WorkflowStateManager _stateManager;
    private readonly IIdGenerator _idGenerator;

    public WorkflowOrchestrator(
        WorkflowResolver workflowResolver,
        WorkflowExecutor executor,
        WorkflowStateManager stateManager,
        IIdGenerator idGenerator)
    {
        _workflowResolver = workflowResolver;
        _executor = executor;
        _stateManager = stateManager;
        _idGenerator = idGenerator;
    }

    public async Task<CommandResult> OrchestrateAsync(CommandContext context)
    {
        var trace = context.Get<ExecutionTrace>(TracingMiddleware.ContextKeys.ExecutionTrace);

        var workflowSpan = trace?.BeginSpan($"Workflow:{context.Envelope.CommandType}", TraceSpanKind.Workflow);

        var steps = await _workflowResolver.ResolveAsync(context);

        if (steps.Length == 0)
        {
            workflowSpan?.Complete(false, "Workflow resolved zero steps.");
            return CommandResult.Fail(
                context.Envelope.CommandId,
                "Workflow resolved zero steps.",
                "EMPTY_WORKFLOW",
                context.Clock.UtcNowOffset);
        }

        // Deterministic WorkflowId: same commandId + commandType → same workflowId.
        // Enables replay, deduplication, and multi-node consistency.
        var workflowId = _idGenerator.DeterministicGuid(
            context.Envelope.CommandId.ToString(),
            context.Envelope.CommandType,
            "workflow");

        var instance = new WorkflowInstance { WorkflowId = workflowId, CommandContext = context };
        instance.Initialize(steps);

        await _stateManager.TrackAsync(instance);

        try
        {
            var result = await _executor.ExecuteAsync(instance);
            workflowSpan?.Complete(result.Success, result.ErrorMessage);
            return result;
        }
        catch (Exception ex)
        {
            workflowSpan?.Complete(false, ex.Message);
            throw;
        }
        finally
        {
            if (instance.Status is WorkflowStatus.Completed or WorkflowStatus.Faulted)
            {
                await _stateManager.RemoveAsync(instance.WorkflowId);
            }
        }
    }
}
