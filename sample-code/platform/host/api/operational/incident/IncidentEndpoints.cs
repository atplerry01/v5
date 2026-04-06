using Whycespace.Platform.Api.Operational.Incident.Contracts;
using Whycespace.Runtime.Bootstrap;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Operational.Incident;

public static class IncidentEndpoints
{
    public static WebApplication MapIncidentEndpoints(this WebApplication app)
    {
        app.MapPost("/api/incident/create", async (
            CreateIncidentRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"incident.create:trace:{request.Type}:{request.Severity}").ToString("N");
            var incidentId = request.IncidentId ?? idGen.DeterministicGuid($"incident.create:incidentId:{traceId}").ToString();
            var commandType = RuntimeBootstrap.IncidentRoute.ResolveCommandType("create");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"incident.create:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    AggregateId = incidentId,
                    IncidentType = request.Type,
                    Severity = request.Severity,
                    Source = request.Source ?? "platform",
                    AffectedEntityId = Guid.TryParse(request.ReferenceId, out var refId) ? refId : idGen.DeterministicGuid($"incident.create:affectedEntity:{traceId}"),
                    Description = request.Description ?? $"Incident {request.Type}",
                    ReferenceDomain = request.ReferenceDomain,
                    ReferenceEntityId = Guid.TryParse(request.ReferenceId, out var refEntId) ? (Guid?)refEntId : null,
                    SourceCorrelationId = request.CommandId
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"incident.create:correlation:{traceId}").ToString(),
                Timestamp = request.Timestamp ?? clock.UtcNow,
                AggregateId = incidentId
            });

            if (result.Success)
                return Results.Ok(new { status = "CREATED", incidentId, data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Incident");

        return app;
    }
}
