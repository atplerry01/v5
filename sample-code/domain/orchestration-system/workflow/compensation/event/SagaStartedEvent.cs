using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaStartedEvent(
    Guid SagaId,
    Guid WorkflowInstanceId,
    string SagaType
) : DomainEvent;
