using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaCompensationTriggeredEvent(
    Guid SagaId,
    Guid FailedStepId,
    int CompensationStepCount
) : DomainEvent;
