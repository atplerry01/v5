namespace Whycespace.Projections.Business.Execution.Sourcing;

public interface ISourcingViewRepository
{
    Task SaveAsync(SourcingReadModel model, CancellationToken ct = default);
    Task<SourcingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
