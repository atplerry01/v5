namespace Whycespace.Projections.Intelligence.Identity.IdentityIntelligence;

public interface IIdentityIntelligenceViewRepository
{
    Task SaveAsync(IdentityIntelligenceReadModel model, CancellationToken ct = default);
    Task<IdentityIntelligenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
