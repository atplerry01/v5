namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public sealed record InstanceCreatedEvent(InstanceId InstanceId, InstanceContext Context);

public sealed record InstanceStartedEvent(InstanceId InstanceId);

public sealed record InstanceCompletedEvent(InstanceId InstanceId);

public sealed record InstanceFailedEvent(InstanceId InstanceId);

public sealed record InstanceTerminatedEvent(InstanceId InstanceId);
