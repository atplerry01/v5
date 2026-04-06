namespace Whycespace.Projections.Trust.Identity.Trust;

public interface ITrustViewRepository
{
    Task SaveAsync(TrustReadModel model, CancellationToken ct = default);
    Task<TrustReadModel?> GetAsync(string id, CancellationToken ct = default);
}
