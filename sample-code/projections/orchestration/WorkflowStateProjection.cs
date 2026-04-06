using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Orchestration;

/// <summary>
/// CQRS projection for workflow execution state visibility.
/// Maintains read models for workflow instances, step progress, and execution metrics.
/// </summary>
public sealed class WorkflowStateProjectionHandler
{
    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    private static readonly HashSet<string> HandledEvents =
    [
        "WorkflowStartedEvent",
        "WorkflowStepCompletedEvent",
        "WorkflowCompletedEvent",
        "WorkflowFailedEvent",
        "WorkflowCompensatedEvent"
    ];

    public WorkflowStateProjectionHandler(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public bool CanHandle(string eventType) => HandledEvents.Contains(eventType);

    public async Task HandleAsync(
        string eventType,
        JsonElement eventData,
        CancellationToken cancellationToken)
    {
        var workflowId = eventData.GetProperty("WorkflowId").GetGuid();
        var key = workflowId.ToString();

        var existing = await _store.GetAsync<WorkflowStateReadModel>("workflow.state", key, cancellationToken);
        var model = existing ?? new WorkflowStateReadModel { WorkflowId = workflowId };

        switch (eventType)
        {
            case "WorkflowStartedEvent":
                model.Status = "Running";
                model.StartedAt = _clock.UtcNowOffset;
                model.WorkflowType = eventData.TryGetProperty("WorkflowType", out var wfType)
                    ? wfType.GetString() ?? "unknown" : "unknown";
                break;

            case "WorkflowStepCompletedEvent":
                model.CompletedSteps++;
                model.CurrentStep = eventData.TryGetProperty("StepName", out var stepName)
                    ? stepName.GetString() : null;
                break;

            case "WorkflowCompletedEvent":
                model.Status = "Completed";
                model.CompletedAt = _clock.UtcNowOffset;
                break;

            case "WorkflowFailedEvent":
                model.Status = "Failed";
                model.CompletedAt = _clock.UtcNowOffset;
                model.ErrorMessage = eventData.TryGetProperty("Reason", out var reason)
                    ? reason.GetString() : null;
                break;

            case "WorkflowCompensatedEvent":
                model.Status = "Compensated";
                model.CompletedAt = _clock.UtcNowOffset;
                break;
        }

        model.LastUpdated = _clock.UtcNowOffset;
        await _store.SetAsync("workflow.state", key, model, cancellationToken);
    }
}

public sealed class WorkflowStateReadModel
{
    public Guid WorkflowId { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? CurrentStep { get; set; }
    public int CompletedSteps { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
