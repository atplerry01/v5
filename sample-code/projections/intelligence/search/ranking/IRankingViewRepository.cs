namespace Whycespace.Projections.Intelligence.Search.Ranking;

public interface IRankingViewRepository
{
    Task SaveAsync(RankingReadModel model, CancellationToken ct = default);
    Task<RankingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
