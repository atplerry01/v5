namespace Whycespace.Domain.IntelligenceSystem.Economic.Integrity;

public sealed class IntegrityValidSpec : Specification<IntelligenceIntegrityAggregate>
{
    public override bool IsSatisfiedBy(IntelligenceIntegrityAggregate entity) =>
        entity.IdentityId != Guid.Empty
        && !string.IsNullOrWhiteSpace(entity.CorrelationId)
        && entity.Window is not null
        && entity.Window.WindowStart <= entity.Window.WindowEnd
        && entity.Scope is not null
        && entity.IntegrityScore >= 0
        && entity.IntegrityScore <= 1
        && entity.CalibratedConfidence >= 0
        && entity.CalibratedConfidence <= 1;
}
