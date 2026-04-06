namespace Whycespace.Projections.Business.Execution.Allocation;

public interface IAllocationViewRepository
{
    Task SaveAsync(AllocationReadModel model, CancellationToken ct = default);
    Task<AllocationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
