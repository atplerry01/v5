namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed class AnalysisValidSpec : Specification<EconomicAnalysisAggregate>
{
    public override bool IsSatisfiedBy(EconomicAnalysisAggregate entity) =>
        entity.IdentityId != Guid.Empty
        && entity.TotalVolume.Value >= 0
        && entity.TransactionCount.Value >= 0
        && !string.IsNullOrWhiteSpace(entity.CorrelationId)
        && !string.IsNullOrWhiteSpace(entity.SourceEventId)
        && entity.Window is not null
        && entity.Window.WindowStart <= entity.Window.WindowEnd
        && entity.Scope is not null;
}
