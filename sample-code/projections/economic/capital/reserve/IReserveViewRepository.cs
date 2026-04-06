namespace Whycespace.Projections.Economic.Capital.Reserve;

public interface IReserveViewRepository
{
    Task SaveAsync(ReserveReadModel model, CancellationToken ct = default);
    Task<ReserveReadModel?> GetAsync(string id, CancellationToken ct = default);
}
