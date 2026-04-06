using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Retry.PolicyAnchor;

/// <summary>
/// In-memory retry queue for failed policy decision anchoring.
/// Items are dequeued by a background retry job.
/// Thread-safe via ConcurrentQueue.
/// </summary>
public sealed class PolicyAnchorRetryQueue
{
    private readonly ConcurrentQueue<PolicyAnchorRetryItem> _queue = new();
    private readonly IClock _clock;

    public PolicyAnchorRetryQueue(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void Enqueue(PolicyAnchorRequest request)
    {
        _queue.Enqueue(new PolicyAnchorRetryItem
        {
            Request = request,
            FailedAt = _clock.UtcNowOffset,
            RetryCount = 0
        });
    }

    public bool TryDequeue(out PolicyAnchorRetryItem? item)
    {
        return _queue.TryDequeue(out item);
    }

    public int Count => _queue.Count;

    public IReadOnlyList<PolicyAnchorRetryItem> PeekAll()
        => _queue.ToArray().ToList().AsReadOnly();
}

public sealed record PolicyAnchorRetryItem
{
    public required PolicyAnchorRequest Request { get; init; }
    public required DateTimeOffset FailedAt { get; init; }
    public required int RetryCount { get; init; }
}
