using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public sealed record WorkflowExecutionFailedEvent(
    AggregateId AggregateId,
    string FailedStepName,
    string Reason) : DomainEvent;
