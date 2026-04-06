using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Dispute;

public sealed record DisputeRaisedEvent(
    Guid DisputeId,
    Guid RelatedDecisionId,
    Guid InitiatorIdentityId) : DomainEvent;
