using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaFailedEvent(
    Guid SagaId,
    Guid FailedStepId,
    string Reason
) : DomainEvent;
