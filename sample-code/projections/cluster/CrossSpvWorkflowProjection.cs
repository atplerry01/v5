using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Cluster;

/// <summary>
/// E18.6.6 — CQRS projection for cross-SPV workflow state.
/// Consumes CrossSpvStateChangedEvent and maintains a read model.
/// </summary>
public sealed class CrossSpvWorkflowProjectionHandler
{
    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    public CrossSpvWorkflowProjectionHandler(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public string EventType => "CrossSpvStateChangedEvent";

    public async Task HandleAsync(
        JsonElement eventData,
        JsonElement metadata,
        CancellationToken cancellationToken)
    {
        var transactionId = eventData.GetProperty("TransactionId").GetGuid();
        var newState = eventData.GetProperty("NewState").GetString() ?? "unknown";
        var key = transactionId.ToString();

        var projection = new CrossSpvWorkflowReadModel
        {
            TransactionId = transactionId,
            Status = newState,
            LastUpdated = _clock.UtcNowOffset
        };

        await _store.SetAsync("crossspv.workflow", key, projection, cancellationToken);
    }
}

public sealed class CrossSpvWorkflowReadModel
{
    public Guid TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset LastUpdated { get; set; }
}
