namespace Whycespace.Projections.Business.Entitlement.EntitlementGrant;

public interface IEntitlementGrantViewRepository
{
    Task SaveAsync(EntitlementGrantReadModel model, CancellationToken ct = default);
    Task<EntitlementGrantReadModel?> GetAsync(string id, CancellationToken ct = default);
}
