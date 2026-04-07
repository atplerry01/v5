using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public sealed record WorkflowExecutionCompletedEvent(
    AggregateId AggregateId,
    string ExecutionHash) : DomainEvent;
