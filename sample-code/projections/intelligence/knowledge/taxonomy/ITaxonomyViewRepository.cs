namespace Whycespace.Projections.Intelligence.Knowledge.Taxonomy;

public interface ITaxonomyViewRepository
{
    Task SaveAsync(TaxonomyReadModel model, CancellationToken ct = default);
    Task<TaxonomyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
