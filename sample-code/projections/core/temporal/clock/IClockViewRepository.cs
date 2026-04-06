namespace Whycespace.Projections.Core.Temporal.Clock;

public interface IClockViewRepository
{
    Task SaveAsync(ClockReadModel model, CancellationToken ct = default);
    Task<ClockReadModel?> GetAsync(string id, CancellationToken ct = default);
}
