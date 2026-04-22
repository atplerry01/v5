namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public sealed record InstanceCreatedEvent(InstanceId InstanceId, InstanceContext Context);
