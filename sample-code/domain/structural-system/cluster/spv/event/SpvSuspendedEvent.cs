using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvSuspendedEvent(Guid SpvId, string Reason) : DomainEvent;
