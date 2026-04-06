namespace Whycespace.Projections.Business.Entitlement.Revocation;

public interface IRevocationViewRepository
{
    Task SaveAsync(RevocationReadModel model, CancellationToken ct = default);
    Task<RevocationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
