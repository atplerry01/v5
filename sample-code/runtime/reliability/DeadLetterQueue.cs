using System.Collections.Concurrent;
using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Reliability;

public sealed record DeadLetterEntry
{
    public Guid EntryId { get; init; }
    public required CommandEnvelope Envelope { get; init; }
    public required string Reason { get; init; }
    public string? ErrorCode { get; init; }
    public Exception? Exception { get; init; }
    public List<RetryAttempt>? RetryAttempts { get; init; }
    public DateTimeOffset RecordedAt { get; init; }
}

public sealed class DeadLetterQueue
{
    private readonly ConcurrentQueue<DeadLetterEntry> _entries = new();
    private readonly ConcurrentDictionary<Guid, DeadLetterEntry> _index = new();

    public int Count => _entries.Count;

    public void Enqueue(DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Enqueue(entry);
        _index[entry.EntryId] = entry;
    }

    public void Enqueue(CommandEnvelope envelope, string reason, Exception? exception = null, List<RetryAttempt>? retryAttempts = null)
    {
        Enqueue(new DeadLetterEntry
        {
            Envelope = envelope,
            Reason = reason,
            Exception = exception,
            RetryAttempts = retryAttempts
        });
    }

    public DeadLetterEntry? Dequeue()
    {
        if (_entries.TryDequeue(out var entry))
        {
            _index.TryRemove(entry.EntryId, out _);
            return entry;
        }

        return null;
    }

    public DeadLetterEntry? Peek()
    {
        _entries.TryPeek(out var entry);
        return entry;
    }

    public DeadLetterEntry? GetById(Guid entryId)
    {
        _index.TryGetValue(entryId, out var entry);
        return entry;
    }

    public IReadOnlyList<DeadLetterEntry> GetAll()
    {
        return [.. _entries];
    }
}
