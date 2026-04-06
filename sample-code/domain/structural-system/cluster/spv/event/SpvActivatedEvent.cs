using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvActivatedEvent(Guid SpvId) : DomainEvent;
