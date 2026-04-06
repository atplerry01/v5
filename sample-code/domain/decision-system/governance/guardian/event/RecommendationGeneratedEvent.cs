using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

public sealed record RecommendationGeneratedEvent(
    Guid RecommendationId,
    string Source,
    int AffectedPolicyCount,
    double ConfidenceScore) : DomainEvent;
