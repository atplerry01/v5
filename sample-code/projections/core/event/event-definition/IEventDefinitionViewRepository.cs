namespace Whycespace.Projections.Core.Event.EventDefinition;

public interface IEventDefinitionViewRepository
{
    Task SaveAsync(EventDefinitionReadModel model, CancellationToken ct = default);
    Task<EventDefinitionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
