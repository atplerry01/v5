using System.Text.Json;
using Whycespace.Projections.Economic;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Workflow.Handlers;

/// <summary>
/// Projects workflow ↔ policy decision links from anchored events with workflow context.
/// Indexes by decisionHash, workflowId, and stepId for audit queries.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class WorkflowPolicyProjectionHandler : IdempotentEconomicProjectionHandler
{
    public WorkflowPolicyProjectionHandler(IClock clock) : base(clock) { }

    public override string ProjectionName => "workflow.policy";

    public override string[] EventTypes =>
    [
        "whyce.observability.policy.decision.anchored"
    ];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var workflowId = GetString(json.Value, "WorkflowId");
        if (string.IsNullOrWhiteSpace(workflowId)) return;

        var decisionHash = GetString(json.Value, "DecisionHash");
        if (decisionHash is null) return;

        var stepId = GetString(json.Value, "StepId") ?? "";

        var model = new WorkflowPolicyReadModel
        {
            DecisionHash = decisionHash,
            WorkflowId = workflowId,
            StepId = stepId,
            State = GetString(json.Value, "WorkflowState") ?? "",
            Transition = GetString(json.Value, "Transition") ?? "",
            PolicyId = GetString(json.Value, "PolicyId") ?? "",
            Decision = GetString(json.Value, "Decision") ?? "",
            SubjectId = GetString(json.Value, "SubjectId") ?? "",
            BlockId = GetString(json.Value, "BlockId"),
            BlockHash = GetString(json.Value, "BlockHash"),
            AnchoredAt = @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await store.SetAsync(ProjectionName, WorkflowPolicyReadModel.KeyFor(decisionHash), model, cancellationToken);

        // Index by workflowId
        await UpdateIndexAsync(store, WorkflowPolicyIndexReadModel.KeyByWorkflow(workflowId),
            workflowId, decisionHash, @event.Timestamp, cancellationToken);

        // Index by stepId
        if (!string.IsNullOrWhiteSpace(stepId))
        {
            await UpdateIndexAsync(store, WorkflowPolicyIndexReadModel.KeyByStep(stepId),
                stepId, decisionHash, @event.Timestamp, cancellationToken);
        }
    }

    private async Task UpdateIndexAsync(IProjectionStore store, string indexKey,
        string keyValue, string decisionHash, DateTimeOffset timestamp, CancellationToken ct)
    {
        var index = await store.GetAsync<WorkflowPolicyIndexReadModel>(ProjectionName, indexKey, ct);

        if (index is null)
        {
            index = new WorkflowPolicyIndexReadModel
            {
                IndexKey = keyValue,
                DecisionHashes = [decisionHash],
                Count = 1,
                LastUpdated = timestamp
            };
        }
        else if (!index.DecisionHashes.Contains(decisionHash))
        {
            index = index with
            {
                DecisionHashes = [.. index.DecisionHashes, decisionHash],
                Count = index.Count + 1,
                LastUpdated = timestamp
            };
        }

        await store.SetAsync(ProjectionName, indexKey, index, ct);
    }

    private static JsonElement? ParsePayload(ProjectionEvent @event)
    {
        if (@event.Payload is JsonElement je) return je;
        if (@event.Payload is null) return null;
        var s = JsonSerializer.Serialize(@event.Payload);
        return JsonDocument.Parse(s).RootElement;
    }

    private static string? GetString(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) ? v.GetString() : null;
}
