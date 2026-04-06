namespace Whycespace.Projections.Constitutional.Policy.Rule;

public interface IRuleViewRepository
{
    Task SaveAsync(RuleReadModel model, CancellationToken ct = default);
    Task<RuleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
