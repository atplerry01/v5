namespace Whycespace.Projections.Business.Scheduler.Availability;

public interface IAvailabilityViewRepository
{
    Task SaveAsync(AvailabilityReadModel model, CancellationToken ct = default);
    Task<AvailabilityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
