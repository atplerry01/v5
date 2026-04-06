using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public sealed record WorkflowDefinitionCreatedEvent(Guid WorkflowDefinitionId) : DomainEvent;
