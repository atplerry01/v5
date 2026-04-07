using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public sealed record WorkflowStepCompletedEvent(
    AggregateId AggregateId,
    int StepIndex,
    string StepName,
    string ExecutionHash,
    object? Output = null,
    string? OutputType = null) : DomainEvent;
