using Whyce.Shared.Contracts.Infrastructure.Messaging;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// In-memory outbox preserving enqueue order. Test substitute for the
/// Postgres outbox + Kafka relay. Captures the topic alongside each batch
/// so tests can assert on TopicNameResolver output.
/// </summary>
public sealed class InMemoryOutbox : IOutbox
{
    private readonly object _lock = new();
    private readonly List<Batch> _batches = new();
    private readonly StageRecorder? _recorder;

    public InMemoryOutbox(StageRecorder? recorder = null)
    {
        _recorder = recorder;
    }

    public IReadOnlyList<Batch> Batches
    {
        get { lock (_lock) return _batches.ToArray(); }
    }

    public Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events, string topic, CancellationToken cancellationToken = default)
    {
        lock (_lock) _batches.Add(new Batch(correlationId, events.ToArray(), topic));
        _recorder?.Record("Outbox");
        return Task.CompletedTask;
    }

    public sealed record Batch(Guid CorrelationId, IReadOnlyList<object> Events, string Topic);
}
