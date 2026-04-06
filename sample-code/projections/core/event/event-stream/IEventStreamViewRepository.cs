namespace Whycespace.Projections.Core.Event.EventStream;

public interface IEventStreamViewRepository
{
    Task SaveAsync(EventStreamReadModel model, CancellationToken ct = default);
    Task<EventStreamReadModel?> GetAsync(string id, CancellationToken ct = default);
}
