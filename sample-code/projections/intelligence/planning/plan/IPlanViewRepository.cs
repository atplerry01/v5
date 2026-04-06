namespace Whycespace.Projections.Intelligence.Planning.Plan;

public interface IPlanViewRepository
{
    Task SaveAsync(PlanReadModel model, CancellationToken ct = default);
    Task<PlanReadModel?> GetAsync(string id, CancellationToken ct = default);
}
