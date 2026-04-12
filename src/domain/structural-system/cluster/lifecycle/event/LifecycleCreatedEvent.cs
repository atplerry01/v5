namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed record LifecycleDefinedEvent(LifecycleId LifecycleId, LifecycleDescriptor Descriptor);

public sealed record LifecycleTransitionedEvent(LifecycleId LifecycleId);

public sealed record LifecycleCompletedEvent(LifecycleId LifecycleId);
