namespace Whycespace.Projections.Business.Entitlement.Restriction;

public interface IRestrictionViewRepository
{
    Task SaveAsync(RestrictionReadModel model, CancellationToken ct = default);
    Task<RestrictionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
