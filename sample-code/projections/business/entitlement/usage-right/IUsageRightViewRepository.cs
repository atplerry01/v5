namespace Whycespace.Projections.Business.Entitlement.UsageRight;

public interface IUsageRightViewRepository
{
    Task SaveAsync(UsageRightReadModel model, CancellationToken ct = default);
    Task<UsageRightReadModel?> GetAsync(string id, CancellationToken ct = default);
}
