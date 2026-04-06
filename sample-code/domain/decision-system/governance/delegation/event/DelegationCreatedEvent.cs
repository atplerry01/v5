using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Delegation;

public sealed record DelegationCreatedEvent(Guid DelegationId, Guid DelegatorIdentityId, Guid DelegateeIdentityId) : DomainEvent;
