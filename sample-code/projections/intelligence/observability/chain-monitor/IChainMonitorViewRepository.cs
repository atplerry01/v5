namespace Whycespace.Projections.Intelligence.Observability.ChainMonitor;

public interface IChainMonitorViewRepository
{
    Task SaveAsync(ChainMonitorReadModel model, CancellationToken ct = default);
    Task<ChainMonitorReadModel?> GetAsync(string id, CancellationToken ct = default);
}
