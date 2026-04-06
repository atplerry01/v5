using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Dispute;

public sealed record DisputeResolvedEvent(
    Guid DisputeId,
    DisputeStatus FinalStatus,
    string Verdict) : DomainEvent;
