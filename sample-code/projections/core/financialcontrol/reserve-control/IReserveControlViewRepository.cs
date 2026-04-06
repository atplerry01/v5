namespace Whycespace.Projections.Core.Financialcontrol.ReserveControl;

public interface IReserveControlViewRepository
{
    Task SaveAsync(ReserveControlReadModel model, CancellationToken ct = default);
    Task<ReserveControlReadModel?> GetAsync(string id, CancellationToken ct = default);
}
