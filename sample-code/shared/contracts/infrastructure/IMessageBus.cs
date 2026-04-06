namespace Whycespace.Shared.Contracts.Infrastructure;

/// <summary>
/// Transport-agnostic message bus contract.
/// Production implementation backed by Kafka.
/// </summary>
public interface IMessageBus
{
    Task PublishAsync(string topic, string partitionKey, byte[] payload, CancellationToken cancellationToken = default);
    Task PublishAsync(string topic, string partitionKey, byte[] payload, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken = default);
}
