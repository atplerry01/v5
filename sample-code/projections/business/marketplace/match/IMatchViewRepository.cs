namespace Whycespace.Projections.Business.Marketplace.Match;

public interface IMatchViewRepository
{
    Task SaveAsync(MatchReadModel model, CancellationToken ct = default);
    Task<MatchReadModel?> GetAsync(string id, CancellationToken ct = default);
}
