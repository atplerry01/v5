using Whycespace.Projections.Economic;

namespace Whycespace.Projections.Economic.Transaction.Limit;

public interface ILimitViewRepository
{
    Task SaveAsync(LimitUsageReadModel model, CancellationToken ct = default);
    Task<LimitUsageReadModel?> GetAsync(string id, CancellationToken ct = default);
}
