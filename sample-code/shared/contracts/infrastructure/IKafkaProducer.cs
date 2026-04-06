namespace Whycespace.Shared.Contracts.Infrastructure;

/// <summary>
/// Transport-agnostic message producer for outbox publishing.
/// Implementations must guarantee idempotent, ordered delivery.
/// </summary>
public interface IMessageProducer
{
    /// <summary>
    /// Publishes a message to a topic with a partition key for ordering.
    /// </summary>
    /// <param name="topic">Target topic.</param>
    /// <param name="key">Partition key — same key guarantees ordering.</param>
    /// <param name="payload">Serialized event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(string topic, string key, string payload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Backward compatibility alias. Use IMessageProducer for new code.
/// </summary>
public interface IKafkaProducer : IMessageProducer;
