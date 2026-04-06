namespace Whycespace.Projections.Business.Agreement.Clause;

public interface IClauseViewRepository
{
    Task SaveAsync(ClauseReadModel model, CancellationToken ct = default);
    Task<ClauseReadModel?> GetAsync(string id, CancellationToken ct = default);
}
