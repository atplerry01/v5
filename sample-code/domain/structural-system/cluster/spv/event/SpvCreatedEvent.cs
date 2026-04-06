using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvCreatedEvent(Guid SpvId) : DomainEvent;
