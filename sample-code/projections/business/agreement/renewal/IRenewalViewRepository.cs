namespace Whycespace.Projections.Business.Agreement.Renewal;

public interface IRenewalViewRepository
{
    Task SaveAsync(RenewalReadModel model, CancellationToken ct = default);
    Task<RenewalReadModel?> GetAsync(string id, CancellationToken ct = default);
}
