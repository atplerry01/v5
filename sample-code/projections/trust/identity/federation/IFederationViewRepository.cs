namespace Whycespace.Projections.Trust.Identity.Federation;

public interface IFederationViewRepository
{
    Task SaveAsync(FederationReadModel model, CancellationToken ct = default);
    Task<FederationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
