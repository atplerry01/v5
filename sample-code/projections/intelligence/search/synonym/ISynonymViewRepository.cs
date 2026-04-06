namespace Whycespace.Projections.Intelligence.Search.Synonym;

public interface ISynonymViewRepository
{
    Task SaveAsync(SynonymReadModel model, CancellationToken ct = default);
    Task<SynonymReadModel?> GetAsync(string id, CancellationToken ct = default);
}
