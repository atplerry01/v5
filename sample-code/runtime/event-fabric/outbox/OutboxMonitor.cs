using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.EventFabric.Outbox;

/// <summary>
/// Observability metrics for the outbox and DLQ pipeline.
/// Tracks pending, processing, failed, and dead-lettered counts.
/// </summary>
public sealed class OutboxMonitor
{
    private readonly IClock _clock;
    private long _pendingCount;

    public OutboxMonitor(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }
    private long _processingCount;
    private long _publishedCount;
    private long _failedCount;
    private long _dlqCount;
    private long _totalPublishLatencyMs;
    private long _publishCount;

    public long PendingCount => Interlocked.Read(ref _pendingCount);
    public long ProcessingCount => Interlocked.Read(ref _processingCount);
    public long PublishedCount => Interlocked.Read(ref _publishedCount);
    public long FailedCount => Interlocked.Read(ref _failedCount);
    public long DlqCount => Interlocked.Read(ref _dlqCount);

    public double AveragePublishLatencyMs
    {
        get
        {
            var count = Interlocked.Read(ref _publishCount);
            return count == 0 ? 0 : (double)Interlocked.Read(ref _totalPublishLatencyMs) / count;
        }
    }

    public void RecordPending() => Interlocked.Increment(ref _pendingCount);
    public void RecordProcessing()
    {
        Interlocked.Increment(ref _processingCount);
        Interlocked.Decrement(ref _pendingCount);
    }

    public void RecordPublished(long latencyMs)
    {
        Interlocked.Increment(ref _publishedCount);
        Interlocked.Decrement(ref _processingCount);
        Interlocked.Add(ref _totalPublishLatencyMs, latencyMs);
        Interlocked.Increment(ref _publishCount);
    }

    public void RecordFailed()
    {
        Interlocked.Increment(ref _failedCount);
        Interlocked.Decrement(ref _processingCount);
    }

    public void RecordDeadLettered() => Interlocked.Increment(ref _dlqCount);

    public OutboxMetricsSnapshot GetSnapshot() => new()
    {
        PendingCount = PendingCount,
        ProcessingCount = ProcessingCount,
        PublishedCount = PublishedCount,
        FailedCount = FailedCount,
        DlqCount = DlqCount,
        AveragePublishLatencyMs = AveragePublishLatencyMs,
        CapturedAt = _clock.UtcNowOffset
    };
}

public sealed record OutboxMetricsSnapshot
{
    public long PendingCount { get; init; }
    public long ProcessingCount { get; init; }
    public long PublishedCount { get; init; }
    public long FailedCount { get; init; }
    public long DlqCount { get; init; }
    public double AveragePublishLatencyMs { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
}
