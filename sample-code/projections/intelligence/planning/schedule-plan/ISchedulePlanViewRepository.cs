namespace Whycespace.Projections.Intelligence.Planning.SchedulePlan;

public interface ISchedulePlanViewRepository
{
    Task SaveAsync(SchedulePlanReadModel model, CancellationToken ct = default);
    Task<SchedulePlanReadModel?> GetAsync(string id, CancellationToken ct = default);
}
