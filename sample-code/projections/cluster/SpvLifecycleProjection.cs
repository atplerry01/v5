using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Cluster;

/// <summary>
/// CQRS projection for SPV lifecycle state.
/// Consumes all SPV lifecycle events and maintains a read model.
/// Supports the full lifecycle: Create → Activate → Suspend → Reactivate → Terminate → Close
/// </summary>
public sealed class SpvLifecycleProjectionHandler
{
    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    private static readonly HashSet<string> HandledEvents =
    [
        "SpvCreatedEvent",
        "SpvActivatedEvent",
        "SpvSuspendedEvent",
        "SpvReactivatedEvent",
        "SpvTerminatedEvent",
        "SpvClosedEvent",
        "SpvOperatorAddedEvent",
        "SpvOperatorReplacedEvent"
    ];

    public SpvLifecycleProjectionHandler(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public bool CanHandle(string eventType) => HandledEvents.Contains(eventType);

    public async Task HandleAsync(
        string eventType,
        JsonElement eventData,
        JsonElement metadata,
        CancellationToken cancellationToken)
    {
        var spvId = eventData.GetProperty("SpvId").GetGuid();
        var key = spvId.ToString();

        var existing = await _store.GetAsync<SpvLifecycleReadModel>("spv.lifecycle", key, cancellationToken);
        var model = existing ?? new SpvLifecycleReadModel { SpvId = spvId };

        switch (eventType)
        {
            case "SpvCreatedEvent":
                model.Status = "Created";
                model.CreatedAt = _clock.UtcNowOffset;
                break;

            case "SpvActivatedEvent":
                model.Status = "Active";
                model.ActivatedAt = _clock.UtcNowOffset;
                break;

            case "SpvSuspendedEvent":
                model.Status = "Suspended";
                model.SuspendedAt = _clock.UtcNowOffset;
                model.SuspensionReason = eventData.TryGetProperty("Reason", out var reason)
                    ? reason.GetString() : null;
                break;

            case "SpvReactivatedEvent":
                model.Status = "Active";
                model.ReactivatedAt = _clock.UtcNowOffset;
                break;

            case "SpvTerminatedEvent":
                model.Status = "Terminated";
                model.TerminatedAt = _clock.UtcNowOffset;
                model.TerminationReason = eventData.TryGetProperty("Reason", out var termReason)
                    ? termReason.GetString() : null;
                break;

            case "SpvClosedEvent":
                model.Status = "Closed";
                model.ClosedAt = _clock.UtcNowOffset;
                model.AuditRecordId = eventData.TryGetProperty("AuditRecordId", out var auditId)
                    ? auditId.GetGuid() : null;
                break;

            case "SpvOperatorAddedEvent":
                var addedOperatorId = eventData.GetProperty("OperatorId").GetGuid();
                if (!model.OperatorIds.Contains(addedOperatorId))
                    model.OperatorIds.Add(addedOperatorId);
                break;

            case "SpvOperatorReplacedEvent":
                var oldOperatorId = eventData.GetProperty("OldOperatorId").GetGuid();
                var newOperatorId = eventData.GetProperty("NewOperatorId").GetGuid();
                model.OperatorIds.Remove(oldOperatorId);
                if (!model.OperatorIds.Contains(newOperatorId))
                    model.OperatorIds.Add(newOperatorId);
                break;
        }

        model.LastUpdated = _clock.UtcNowOffset;
        model.EventCount++;

        await _store.SetAsync("spv.lifecycle", key, model, cancellationToken);
    }
}

public sealed class SpvLifecycleReadModel
{
    public Guid SpvId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTimeOffset? ReactivatedAt { get; set; }
    public DateTimeOffset? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? AuditRecordId { get; set; }
    public List<Guid> OperatorIds { get; set; } = [];
    public DateTimeOffset LastUpdated { get; set; }
    public int EventCount { get; set; }
}
