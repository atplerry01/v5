using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public sealed record WorkflowExecutionStartedEvent(
    AggregateId AggregateId,
    string WorkflowName,
    object? Payload = null,
    string? PayloadType = null) : DomainEvent;
