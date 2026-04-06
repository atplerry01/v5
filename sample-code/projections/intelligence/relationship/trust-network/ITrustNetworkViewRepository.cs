namespace Whycespace.Projections.Intelligence.Relationship.TrustNetwork;

public interface ITrustNetworkViewRepository
{
    Task SaveAsync(TrustNetworkReadModel model, CancellationToken ct = default);
    Task<TrustNetworkReadModel?> GetAsync(string id, CancellationToken ct = default);
}
