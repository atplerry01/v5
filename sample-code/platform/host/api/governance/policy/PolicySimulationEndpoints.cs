using Whycespace.Runtime.Bootstrap;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Governance.Policy;

public static class PolicySimulationEndpoints
{
    public static WebApplication MapPolicySimulationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/policy/simulate", async (
            SimulatePolicyRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.simulate:trace:{request.PolicyId}:{request.ActorId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("simulate");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.simulate:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    PolicyId = request.PolicyId,
                    request.Version,
                    request.ActorId,
                    request.Action,
                    request.Resource,
                    request.Environment,
                    request.SimulatedTime,
                    request.IncludeImpactAnalysis,
                    request.IncludeRiskScoring,
                    request.IncludeAnomalyDetection,
                    request.SnapshotId,
                    request.Seed,
                    request.RunCount,
                    request.IncludeConfidenceScoring,
                    request.IncludeDriftDetection
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"policy.simulate:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "SIMULATED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy Simulation");

        app.MapPost("/api/policy/simulate/batch", async (
            BatchSimulatePolicyRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.simulate.batch:trace:{request.Scenarios.Count}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("simulate.batch");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.simulate.batch:command:{traceId}"),
                CommandType = commandType,
                Payload = request,
                CorrelationId = idGen.DeterministicGuid($"policy.simulate.batch:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "BATCH_SIMULATED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy Simulation");

        return app;
    }
}

public sealed record SimulatePolicyRequest
{
    public required string PolicyId { get; init; }
    public int? Version { get; init; }
    public required string ActorId { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public string? Environment { get; init; }
    public DateTimeOffset? SimulatedTime { get; init; }
    public bool IncludeImpactAnalysis { get; init; } = true;
    public bool IncludeRiskScoring { get; init; } = true;
    public bool IncludeAnomalyDetection { get; init; } = true;
    public string? CommandId { get; init; }

    // E12.1 — Hardening extensions (additive, backward-compatible)
    public string? SnapshotId { get; init; }
    public int? Seed { get; init; }
    public int RunCount { get; init; } = 1;
    public bool IncludeConfidenceScoring { get; init; } = true;
    public bool IncludeDriftDetection { get; init; } = true;
}

public sealed record BatchSimulatePolicyRequest
{
    public required IReadOnlyList<SimulatePolicyRequest> Scenarios { get; init; }
}
