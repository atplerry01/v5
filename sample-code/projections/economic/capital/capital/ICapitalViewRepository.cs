namespace Whycespace.Projections.Economic.Capital.Capital;

public interface ICapitalViewRepository
{
    Task SaveAsync(CapitalReadModel model, CancellationToken ct = default);
    Task<CapitalReadModel?> GetAsync(string id, CancellationToken ct = default);
}
