using System.Text.Json;
using Whycespace.Projections.Economic;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Simulation.Handlers;

/// <summary>
/// Projects simulation results from whyce.simulation.executed events.
/// Stores results for analytics and historical queries.
/// Idempotent via EventId deduplication (base class).
/// </summary>
public sealed class SimulationProjectionHandler : IdempotentEconomicProjectionHandler
{
    public SimulationProjectionHandler(IClock clock) : base(clock) { }

    public override string ProjectionName => "simulation.result";

    public override string[] EventTypes =>
    [
        "whyce.simulation.executed"
    ];

    protected override async Task ApplyAsync(
        ProjectionEvent @event,
        IProjectionStore store,
        CancellationToken cancellationToken)
    {
        var json = ParsePayload(@event);
        if (json is null) return;

        var simulationId = GetString(json.Value, "SimulationId");
        if (simulationId is null) return;

        var model = new SimulationResultReadModel
        {
            SimulationId = simulationId,
            ScenarioType = GetString(json.Value, "ScenarioType") ?? "",
            SubjectId = GetString(json.Value, "SubjectId") ?? "",
            Action = GetString(json.Value, "Action") ?? "",
            Resource = GetString(json.Value, "Resource") ?? "",
            PolicyAllowed = GetBool(json.Value, "PolicyAllowed"),
            PredictedDecision = GetString(json.Value, "PredictedDecision") ?? "",
            RiskScore = GetDouble(json.Value, "RiskScore"),
            RiskCategory = GetString(json.Value, "RiskCategory"),
            RecommendationSummary = GetString(json.Value, "RecommendationSummary"),
            RecommendationCount = (int)GetDouble(json.Value, "RecommendationCount"),
            Amount = GetNullableDecimal(json.Value, "Amount"),
            Currency = GetString(json.Value, "Currency"),
            WorkflowId = GetString(json.Value, "WorkflowId"),
            SimulatedAt = @event.Timestamp,
            LastUpdated = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await store.SetAsync(ProjectionName, SimulationResultReadModel.KeyFor(simulationId), model, cancellationToken);
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

    private static decimal? GetNullableDecimal(JsonElement json, string prop)
        => json.TryGetProperty(prop, out var v) && v.TryGetDecimal(out var d) ? d : null;
}
