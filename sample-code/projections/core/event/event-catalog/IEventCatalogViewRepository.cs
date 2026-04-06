namespace Whycespace.Projections.Core.Event.EventCatalog;

public interface IEventCatalogViewRepository
{
    Task SaveAsync(EventCatalogReadModel model, CancellationToken ct = default);
    Task<EventCatalogReadModel?> GetAsync(string id, CancellationToken ct = default);
}
