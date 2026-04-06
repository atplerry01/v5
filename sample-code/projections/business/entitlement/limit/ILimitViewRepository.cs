namespace Whycespace.Projections.Business.Entitlement.Limit;

public interface ILimitViewRepository
{
    Task SaveAsync(LimitReadModel model, CancellationToken ct = default);
    Task<LimitReadModel?> GetAsync(string id, CancellationToken ct = default);
}
