using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaCompletedEvent(
    Guid SagaId,
    int TotalStepsCompleted
) : DomainEvent;
