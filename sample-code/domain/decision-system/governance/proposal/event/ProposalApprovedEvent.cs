using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public sealed record ProposalApprovedEvent(Guid ProposalId) : DomainEvent;
