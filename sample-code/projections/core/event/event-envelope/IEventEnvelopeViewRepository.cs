namespace Whycespace.Projections.Core.Event.EventEnvelope;

public interface IEventEnvelopeViewRepository
{
    Task SaveAsync(EventEnvelopeReadModel model, CancellationToken ct = default);
    Task<EventEnvelopeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
