using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaStepCompletedEvent(
    Guid SagaId,
    Guid StepId
) : DomainEvent;
