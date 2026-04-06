namespace Whycespace.Projections.Business.Subscription.Plan;

public interface IPlanViewRepository
{
    Task SaveAsync(PlanReadModel model, CancellationToken ct = default);
    Task<PlanReadModel?> GetAsync(string id, CancellationToken ct = default);
}
