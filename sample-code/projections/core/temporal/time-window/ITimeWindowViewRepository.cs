namespace Whycespace.Projections.Core.Temporal.TimeWindow;

public interface ITimeWindowViewRepository
{
    Task SaveAsync(TimeWindowReadModel model, CancellationToken ct = default);
    Task<TimeWindowReadModel?> GetAsync(string id, CancellationToken ct = default);
}
