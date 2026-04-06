using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record PolicyProposalCreatedEvent(
    Guid ProposalId,
    Guid PolicyId,
    int ProposedVersion) : DomainEvent;
