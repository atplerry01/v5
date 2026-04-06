using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Step;

public sealed record WorkflowStepCreatedEvent(Guid WorkflowStepId) : DomainEvent;
