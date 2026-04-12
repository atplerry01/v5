namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public sealed record LifecycleCreatedEvent(LifecycleId LifecycleId, LifecycleSubjectId SubjectId);
