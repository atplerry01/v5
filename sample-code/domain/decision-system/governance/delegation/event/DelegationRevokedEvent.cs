using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Delegation;

public sealed record DelegationRevokedEvent(Guid DelegationId, Guid RevokerIdentityId) : DomainEvent;
