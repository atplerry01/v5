namespace Whycespace.Projections.Business.Localization.RegionalRule;

public interface IRegionalRuleViewRepository
{
    Task SaveAsync(RegionalRuleReadModel model, CancellationToken ct = default);
    Task<RegionalRuleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
