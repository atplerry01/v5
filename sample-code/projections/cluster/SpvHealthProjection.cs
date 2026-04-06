using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Cluster;

/// <summary>
/// CQRS projection for SPV health dashboard.
/// Aggregates lifecycle events across all SPVs for observability.
/// </summary>
public sealed class SpvHealthProjectionHandler
{
    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    public SpvHealthProjectionHandler(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public async Task HandleAsync(
        string eventType,
        JsonElement eventData,
        CancellationToken cancellationToken)
    {
        var key = "spv.health.dashboard";
        var dashboard = await _store.GetAsync<SpvHealthDashboardReadModel>(key, key, cancellationToken)
            ?? new SpvHealthDashboardReadModel();

        switch (eventType)
        {
            case "SpvCreatedEvent":
                dashboard.TotalCount++;
                dashboard.CreatedCount++;
                break;
            case "SpvActivatedEvent":
                dashboard.CreatedCount = Math.Max(0, dashboard.CreatedCount - 1);
                dashboard.ActiveCount++;
                break;
            case "SpvSuspendedEvent":
                dashboard.ActiveCount = Math.Max(0, dashboard.ActiveCount - 1);
                dashboard.SuspendedCount++;
                break;
            case "SpvReactivatedEvent":
                dashboard.SuspendedCount = Math.Max(0, dashboard.SuspendedCount - 1);
                dashboard.ActiveCount++;
                break;
            case "SpvTerminatedEvent":
                dashboard.ActiveCount = Math.Max(0, dashboard.ActiveCount - 1);
                dashboard.SuspendedCount = Math.Max(0, dashboard.SuspendedCount - 1);
                dashboard.TerminatedCount++;
                break;
            case "SpvClosedEvent":
                dashboard.TerminatedCount = Math.Max(0, dashboard.TerminatedCount - 1);
                dashboard.ClosedCount++;
                break;
        }

        dashboard.LastUpdated = _clock.UtcNowOffset;
        await _store.SetAsync(key, key, dashboard, cancellationToken);
    }
}

public sealed class SpvHealthDashboardReadModel
{
    public int TotalCount { get; set; }
    public int CreatedCount { get; set; }
    public int ActiveCount { get; set; }
    public int SuspendedCount { get; set; }
    public int TerminatedCount { get; set; }
    public int ClosedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
