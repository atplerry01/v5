namespace Whycespace.Projections.Business.Integration.EventBridge;

public interface IEventBridgeViewRepository
{
    Task SaveAsync(EventBridgeReadModel model, CancellationToken ct = default);
    Task<EventBridgeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
