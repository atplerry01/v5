using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvOperatorAddedEvent(Guid SpvId, Guid IdentityId) : DomainEvent;
