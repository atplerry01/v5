using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public sealed record ProposalCreatedEvent(Guid ProposalId, Guid ProposerIdentityId) : DomainEvent;
