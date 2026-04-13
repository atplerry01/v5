namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Event Dispatcher — routes EventEnvelopes to registered in-process consumers.
/// Used by the Event Fabric for projection dispatch and observability.
/// </summary>
public sealed class EventDispatcher
{
    private readonly List<IEventEnvelopeConsumer> _consumers = new();

    public void Register(IEventEnvelopeConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        _consumers.Add(consumer);
    }

    public async Task DispatchAsync(IReadOnlyList<EventEnvelope> envelopes)
    {
        foreach (var envelope in envelopes)
        {
            foreach (var consumer in _consumers)
            {
                await consumer.ConsumeAsync(envelope);
            }
        }
    }
}

public interface IEventEnvelopeConsumer
{
    Task ConsumeAsync(EventEnvelope envelope);
}
