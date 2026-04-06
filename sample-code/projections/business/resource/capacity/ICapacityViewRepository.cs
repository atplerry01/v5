namespace Whycespace.Projections.Business.Resource.Capacity;

public interface ICapacityViewRepository
{
    Task SaveAsync(CapacityReadModel model, CancellationToken ct = default);
    Task<CapacityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
