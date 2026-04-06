using Whycespace.Runtime.Bootstrap;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Governance.Policy;

public static class PolicyFederationEndpoints
{
    public static WebApplication MapPolicyFederationEndpoints(this WebApplication app)
    {
        // ── Commands ──

        app.MapPost("/api/policy/federation/build", async (
            BuildFederationGraphRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid("federation.build:trace").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("federation.build");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"federation.build:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    request.Nodes,
                    request.Edges
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"federation.build:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "FEDERATION_GRAPH_BUILT", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy Federation");

        app.MapPost("/api/policy/federation/evaluate", async (
            EvaluateFederationRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"federation.evaluate:trace:{request.GraphHash}:{request.ActorId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("federation.evaluate");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"federation.evaluate:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    request.GraphHash,
                    request.ActorId,
                    request.Action,
                    request.Resource,
                    request.Environment
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"federation.evaluate:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "FEDERATION_EVALUATED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy Federation");

        // ── Queries ──

        app.MapGet("/api/policy/federation/{hash}", async (
            string hash,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"federation.get:trace:{hash}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("federation.get");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"federation.get:command:{traceId}"),
                CommandType = commandType,
                Payload = new { GraphHash = hash },
                CorrelationId = idGen.DeterministicGuid($"federation.get:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { data = result.Data, traceId });

            return Results.NotFound(new { error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy Federation");

        return app;
    }
}

// ── Request DTOs (Platform layer) ──

public sealed record BuildFederationGraphRequest
{
    public required List<FederationNodeInput> Nodes { get; init; }
    public required List<FederationEdgeInput> Edges { get; init; }
    public string? CommandId { get; init; }
}

public sealed record FederationNodeInput
{
    public required Guid PolicyId { get; init; }
    public required int Version { get; init; }
    public required string ClusterId { get; init; }
}

public sealed record FederationEdgeInput
{
    public required Guid SourcePolicyId { get; init; }
    public required Guid TargetPolicyId { get; init; }
    public required string RelationType { get; init; }
}

public sealed record EvaluateFederationRequest
{
    public required string GraphHash { get; init; }
    public required string ActorId { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public string? Environment { get; init; }
    public string? CommandId { get; init; }
}
