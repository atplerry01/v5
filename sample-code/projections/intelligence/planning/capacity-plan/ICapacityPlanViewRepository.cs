namespace Whycespace.Projections.Intelligence.Planning.CapacityPlan;

public interface ICapacityPlanViewRepository
{
    Task SaveAsync(CapacityPlanReadModel model, CancellationToken ct = default);
    Task<CapacityPlanReadModel?> GetAsync(string id, CancellationToken ct = default);
}
