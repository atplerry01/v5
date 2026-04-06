namespace Whycespace.Projections.Intelligence.Relationship.Affiliation;

public interface IAffiliationViewRepository
{
    Task SaveAsync(AffiliationReadModel model, CancellationToken ct = default);
    Task<AffiliationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
