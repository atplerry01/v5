using Whyce.Shared.Contracts.EventFabric;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whyce.Engines.T1M.Lifecycle;

/// <summary>
/// T1M-tier factory that produces workflow lifecycle domain events directly,
/// without invoking mutating methods on <see cref="WorkflowExecutionAggregate"/>.
///
/// Engine-guard rule 3 prohibits T1M from calling aggregate mutation methods.
/// The factory satisfies that constraint by constructing the events itself.
/// The aggregate remains the canonical replay target (its Apply method
/// reconstructs state from these same events).
///
/// The factory consults <see cref="IPayloadTypeRegistry"/> to stamp the
/// PayloadType / OutputType discriminators on lifecycle events when the
/// payload's CLR type has been registered by a domain bootstrap module. The
/// discriminator is what allows the engine-side replay service to rehydrate
/// the typed CLR object from JSON on Postgres-backed replay (rule E-TYPE-02).
/// </summary>
public sealed class WorkflowLifecycleEventFactory
{
    private readonly IPayloadTypeRegistry _payloadTypes;

    public WorkflowLifecycleEventFactory(IPayloadTypeRegistry payloadTypes)
    {
        _payloadTypes = payloadTypes;
    }

    public WorkflowExecutionStartedEvent Started(
        Guid workflowExecutionId, string workflowName, object? payload = null)
        => new(new AggregateId(workflowExecutionId), workflowName, payload, ResolveTypeName(payload));

    public WorkflowStepCompletedEvent StepCompleted(
        Guid workflowExecutionId, int stepIndex, string stepName, string executionHash, object? output = null)
        => new(new AggregateId(workflowExecutionId), stepIndex, stepName, executionHash, output, ResolveTypeName(output));

    private string? ResolveTypeName(object? value)
    {
        if (value is null) return null;
        return _payloadTypes.TryGetName(value.GetType(), out var name) ? name : null;
    }

    public WorkflowExecutionCompletedEvent Completed(Guid workflowExecutionId, string executionHash)
        => new(new AggregateId(workflowExecutionId), executionHash);

    public WorkflowExecutionFailedEvent Failed(
        Guid workflowExecutionId, string failedStepName, string reason)
        => new(new AggregateId(workflowExecutionId), failedStepName, reason);
}
