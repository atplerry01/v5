using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public sealed record WorkflowCompletedEvent(Guid InstanceId) : DomainEvent;
