using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

public sealed record RecommendationReviewedEvent(
    Guid RecommendationId,
    string NewStatus,
    string? ReviewNotes) : DomainEvent;
