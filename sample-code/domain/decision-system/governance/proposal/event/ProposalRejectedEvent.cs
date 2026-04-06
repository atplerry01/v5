using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public sealed record ProposalRejectedEvent(Guid ProposalId) : DomainEvent;
