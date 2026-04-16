using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Systems.Midstream.Wss;

public sealed class WorkflowDispatcher : IWorkflowDispatcher
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;

    public WorkflowDispatcher(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
    }

    public async Task<WorkflowResult> StartWorkflowAsync(string workflowName, object payload, DomainRoute route)
    {
        // phase1.6-S1.1 (DET-SEED-DERIVATION-01): workflow id derives from the
        // workflow name + payload signature only. No clock entropy. Two starts
        // with identical (name, payload) collapse to the same workflow id,
        // which is the correct semantics for at-least-once trigger delivery
        // — the IdempotencyMiddleware downstream will deduplicate. Callers
        // that need distinct workflow instances for identical payloads must
        // include a discriminator inside the payload itself.
        var payloadSignature = payload.ToString() ?? string.Empty;
        var workflowId = _idGenerator.Generate($"workflow:{workflowName}:{payloadSignature}");

        var command = new WorkflowStartCommand(workflowId, workflowName, payload);
        var result = await _dispatcher.DispatchAsync(command, route);

        return (result.IsSuccess
            ? WorkflowResult.Success(result.EmittedEvents, result.Output)
            : WorkflowResult.Failure(result.Error ?? "Workflow execution failed."))
            with
            { CorrelationId = result.CorrelationId };
    }
}
