using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record PolicyProposalApprovedEvent(
    Guid ProposalId,
    Guid ApproverId) : DomainEvent;
