namespace Whycespace.Projections.Core.Event.EventSchema;

public interface IEventSchemaViewRepository
{
    Task SaveAsync(EventSchemaReadModel model, CancellationToken ct = default);
    Task<EventSchemaReadModel?> GetAsync(string id, CancellationToken ct = default);
}
