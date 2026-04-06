using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Orchestration;

/// <summary>
/// CQRS projection for execution metrics aggregation.
/// Tracks workflow throughput, success/failure rates, and processing times.
/// </summary>
public sealed class ExecutionMetricsProjectionHandler
{
    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    public ExecutionMetricsProjectionHandler(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public async Task HandleAsync(
        string eventType,
        JsonElement eventData,
        CancellationToken cancellationToken)
    {
        var key = "execution.metrics.global";
        var metrics = await _store.GetAsync<ExecutionMetricsReadModel>(key, key, cancellationToken)
            ?? new ExecutionMetricsReadModel();

        switch (eventType)
        {
            case "WorkflowStartedEvent":
                metrics.TotalStarted++;
                metrics.CurrentlyRunning++;
                break;
            case "WorkflowCompletedEvent":
                metrics.TotalCompleted++;
                metrics.CurrentlyRunning = Math.Max(0, metrics.CurrentlyRunning - 1);
                break;
            case "WorkflowFailedEvent":
                metrics.TotalFailed++;
                metrics.CurrentlyRunning = Math.Max(0, metrics.CurrentlyRunning - 1);
                break;
            case "WorkflowCompensatedEvent":
                metrics.TotalCompensated++;
                break;
        }

        metrics.LastUpdated = _clock.UtcNowOffset;
        await _store.SetAsync(key, key, metrics, cancellationToken);
    }
}

public sealed class ExecutionMetricsReadModel
{
    public int TotalStarted { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalFailed { get; set; }
    public int TotalCompensated { get; set; }
    public int CurrentlyRunning { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
