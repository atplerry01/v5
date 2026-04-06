namespace Whycespace.Shared.Contracts.Messaging;

/// <summary>
/// Abstract message consumer contract. Hides transport implementation (Kafka, etc.).
/// Infrastructure adapters implement this with specific transport bindings.
/// </summary>
public interface IMessageConsumer : IAsyncDisposable
{
    void Subscribe(string topic);
    void Subscribe(IEnumerable<string> topics);
    Task<ConsumedMessage?> ConsumeAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    void Commit();
}

public sealed record ConsumedMessage(
    string Key,
    string Value,
    string Topic,
    int Partition,
    long Offset);
