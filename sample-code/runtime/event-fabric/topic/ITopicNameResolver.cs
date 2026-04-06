namespace Whycespace.Runtime.EventFabric.Topic;

/// <summary>
/// Resolves canonical Kafka topic names for runtime events.
/// Guarantees: lowercase, no duplicate prefix, correct segment count.
/// </summary>
public interface ITopicNameResolver
{
    string Resolve(RuntimeEvent @event);
}
