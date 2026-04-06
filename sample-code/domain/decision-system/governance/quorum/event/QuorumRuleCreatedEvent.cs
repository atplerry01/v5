using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Quorum;

public sealed record QuorumRuleCreatedEvent(Guid QuorumRuleId) : DomainEvent;
