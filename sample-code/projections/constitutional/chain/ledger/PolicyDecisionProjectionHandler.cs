using System.Text.Json;
using Whycespace.Projections.Economic;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Chain.Handlers;

/// <summary>
/// Projects anchored policy decisions from whyce.observability.policy.decision.anchored events.
/// Indexes decisions by correlationId and policyId for audit queries.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class PolicyDecisionProjectionHandler : IdempotentEconomicProjectionHandler
{
    public PolicyDecisionProjectionHandler(IClock clock) : base(clock) { }

    public override string ProjectionName => "chain.policy-decision";

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

        var correlationId = GetString(json.Value, "CorrelationId");
        var decisionHash = GetString(json.Value, "DecisionHash") ?? correlationId;
        var policyId = GetString(json.Value, "PolicyId");
        var decision = GetString(json.Value, "Decision");
        var subject = GetString(json.Value, "Subject");
        var resource = GetString(json.Value, "Resource");
        var action = GetString(json.Value, "Action");
        var blockId = GetString(json.Value, "BlockId");
        var blockHash = GetString(json.Value, "BlockHash");

        if (decisionHash is null || policyId is null || decision is null) return;

        // E4.1: Key by decisionHash (idempotent key) — ignores duplicate events
        var key = PolicyDecisionReadModel.KeyFor(decisionHash);
        var existing = await store.GetAsync<PolicyDecisionReadModel>(
            ProjectionName, key, cancellationToken);

        if (existing is not null &&
            ShouldSkipEvent(@event.Timestamp, @event.Version,
                existing.LastUpdated, existing.LastEventVersion))
            return;

        // E5: Extract identity fields
        var subjectId = GetString(json.Value, "SubjectId");
        var trustScore = GetDouble(json.Value, "TrustScore");
        var isVerified = GetBool(json.Value, "IsVerified");
        var roles = GetStringArray(json.Value, "Roles");

        var model = new PolicyDecisionReadModel
        {
            CorrelationId = correlationId ?? decisionHash,
            PolicyId = policyId,
            Decision = decision,
            Subject = subject ?? "",
            Resource = resource ?? "",
            Action = action ?? "",
            EvaluationHash = GetString(json.Value, "EvaluationHash") ?? "",
            BlockId = blockId ?? "",
            BlockHash = blockHash ?? "",
            AnchoredAt = @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventVersion = @event.Version,
            SubjectId = subjectId,
            Roles = roles,
            TrustScore = trustScore,
            IsVerified = isVerified
        };

        await store.SetAsync(ProjectionName, key, model, cancellationToken);

        // E5: Index by subjectId
        if (subjectId is not null)
        {
            var subjectKey = PolicyDecisionReadModel.KeyBySubject(subjectId);
            var subjectIndex = await store.GetAsync<PolicyDecisionIndexReadModel>(
                ProjectionName, subjectKey, cancellationToken);

            if (subjectIndex is null)
            {
                subjectIndex = new PolicyDecisionIndexReadModel
                {
                    PolicyId = subjectId,
                    CorrelationIds = [decisionHash],
                    DecisionCount = 1,
                    LastUpdated = @event.Timestamp
                };
            }
            else if (!subjectIndex.CorrelationIds.Contains(decisionHash))
            {
                subjectIndex = subjectIndex with
                {
                    CorrelationIds = [.. subjectIndex.CorrelationIds, decisionHash],
                    DecisionCount = subjectIndex.DecisionCount + 1,
                    LastUpdated = @event.Timestamp
                };
            }

            await store.SetAsync(ProjectionName, subjectKey, subjectIndex, cancellationToken);
        }

        // Update policy index
        var indexKey = PolicyDecisionIndexReadModel.KeyFor(policyId);
        var index = await store.GetAsync<PolicyDecisionIndexReadModel>(
            ProjectionName, indexKey, cancellationToken);

        if (index is null)
        {
            index = new PolicyDecisionIndexReadModel
            {
                PolicyId = policyId,
                CorrelationIds = [correlationId ?? ""],
                DecisionCount = 1,
                LastUpdated = @event.Timestamp
            };
        }
        else if (!index.CorrelationIds.Contains(correlationId ?? ""))
        {
            index = index with
            {
                CorrelationIds = [.. index.CorrelationIds, correlationId ?? ""],
                DecisionCount = index.DecisionCount + 1,
                LastUpdated = @event.Timestamp
            };
        }

        await store.SetAsync(ProjectionName, indexKey, index, cancellationToken);
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

    private static double GetDouble(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetDouble(out var d) ? d : 0.0;

    private static bool GetBool(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;

    private static string[]? GetStringArray(JsonElement json, string prop)
    {
        if (!json.TryGetProperty(prop, out var v) || v.ValueKind != JsonValueKind.Array)
            return null;
        return v.EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
    }
}
