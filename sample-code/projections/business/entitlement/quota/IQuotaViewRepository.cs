namespace Whycespace.Projections.Business.Entitlement.Quota;

public interface IQuotaViewRepository
{
    Task SaveAsync(QuotaReadModel model, CancellationToken ct = default);
    Task<QuotaReadModel?> GetAsync(string id, CancellationToken ct = default);
}
