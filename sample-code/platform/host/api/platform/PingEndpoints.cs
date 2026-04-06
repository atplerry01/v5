using Whycespace.Platform.Api.Platform.Contracts;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Platform;

public static class PingEndpoints
{
    public static WebApplication MapPingEndpoints(this WebApplication app)
    {
        app.MapPost("/api/ping", async (
            PingCommandRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"platform.ping:trace:{request.AggregateId}").ToString("N");
            var handler = registry.Resolve("platform.ping");

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"platform.ping:command:{traceId}"),
                CommandType = "platform.ping",
                Payload = new { request.AggregateId, request.Message },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"platform.ping:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow,
                AggregateId = request.AggregateId
            });

            if (result.Success)
                return Results.Ok(new { status = "ACCEPTED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Platform");

        return app;
    }
}
