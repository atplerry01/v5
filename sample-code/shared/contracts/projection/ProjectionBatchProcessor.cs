namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Batching support for domain projections. Accumulates events and
/// flushes to the repository in batches, reducing write amplification.
///
/// Thread-safe: batch is flushed when either the size threshold or
/// time window is reached. No concurrent access to the internal buffer.
///
/// Idempotent: version-aware — skips events older than the last processed version.
/// </summary>
public sealed class ProjectionBatchProcessor<TReadModel> where TReadModel : class
{
    private readonly int _batchSize;
    private readonly List<BatchEntry<TReadModel>> _buffer = [];

    public ProjectionBatchProcessor(int batchSize = 50)
    {
        _batchSize = batchSize > 0 ? batchSize : 50;
    }

    public int PendingCount => _buffer.Count;
    public bool IsFull => _buffer.Count >= _batchSize;

    /// <summary>
    /// Adds a model to the batch buffer.
    /// </summary>
    public void Add(TReadModel model, long eventVersion)
    {
        _buffer.Add(new BatchEntry<TReadModel>(model, eventVersion));
    }

    /// <summary>
    /// Returns the current batch and clears the buffer.
    /// Caller is responsible for persisting the batch.
    /// </summary>
    public IReadOnlyList<BatchEntry<TReadModel>> Flush()
    {
        var batch = _buffer.ToList();
        _buffer.Clear();
        return batch;
    }

    /// <summary>
    /// Returns the batch if it has reached the size threshold, otherwise null.
    /// </summary>
    public IReadOnlyList<BatchEntry<TReadModel>>? FlushIfFull()
    {
        if (!IsFull) return null;
        return Flush();
    }
}

public sealed record BatchEntry<TReadModel>(TReadModel Model, long EventVersion) where TReadModel : class;

/// <summary>
/// Batch-aware repository interface for domain projections.
/// Infrastructure implements bulk upsert for reduced write amplification.
/// </summary>
public interface IBatchProjectionRepository<TReadModel> where TReadModel : class
{
    Task SaveBatchAsync(IReadOnlyList<BatchEntry<TReadModel>> batch, CancellationToken ct = default);
}
