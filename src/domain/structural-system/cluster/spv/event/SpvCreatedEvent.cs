namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvCreatedEvent(SpvId SpvId, SpvDescriptor Descriptor);

public sealed record SpvActivatedEvent(SpvId SpvId);

public sealed record SpvSuspendedEvent(SpvId SpvId);

public sealed record SpvClosedEvent(SpvId SpvId);
