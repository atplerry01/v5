namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed class OptimizationValidSpec : Specification<EconomicOptimizationAggregate>
{
    public override bool IsSatisfiedBy(EconomicOptimizationAggregate entity) =>
        entity.IdentityId != Guid.Empty
        && entity.RecommendationType is not null
        && !string.IsNullOrWhiteSpace(entity.SuggestedAction)
        && entity.ConfidenceScore.Value >= 0
        && entity.ConfidenceScore.Value <= 1
        && !string.IsNullOrWhiteSpace(entity.CorrelationId)
        && !string.IsNullOrWhiteSpace(entity.SourceEventId)
        && entity.Window is not null
        && entity.Window.WindowStart <= entity.Window.WindowEnd
        && entity.Scope is not null;
}
