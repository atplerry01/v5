using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvOperatorReplacedEvent(
    Guid SpvId,
    Guid OldOperatorId,
    Guid NewOperatorId) : DomainEvent;
