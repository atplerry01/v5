using Whycespace.Platform.Api.Operational.Sandbox.Todo.Contracts;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Operational.Sandbox.Todo;

public static class TodoEndpoints
{
    public static WebApplication MapTodoEndpoints(this WebApplication app)
    {
        app.MapPost("/api/todo", async (
            CreateTodoRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"todo.create:trace:{request.Title}").ToString("N");
            var todoId = request.TodoId ?? idGen.DeterministicGuid($"todo.create:todoId:{traceId}").ToString();
            var commandType = "operational.todo.create";
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"todo.create:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    TodoId = todoId,
                    request.Title,
                    request.Priority,
                    request.AssignedTo
                },
                CorrelationId = idGen.DeterministicGuid($"todo.create:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow,
                AggregateId = todoId
            });

            if (result.Success)
                return Results.Ok(new { status = "CREATED", todoId = result.Data ?? (object)todoId, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Todo");

        app.MapPost("/api/todo/{todoId}/complete", async (
            string todoId,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"todo.complete:trace:{todoId}").ToString("N");

            if (!Guid.TryParse(todoId, out _))
                return Results.BadRequest(new { status = "INVALID", error = "todoId must be a valid GUID", traceId });

            var commandType = "operational.todo.complete";
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"todo.complete:command:{traceId}"),
                CommandType = commandType,
                Payload = new { TodoId = todoId },
                CorrelationId = idGen.DeterministicGuid($"todo.complete:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow,
                AggregateId = todoId
            });

            if (result.Success)
                return Results.Ok(new { status = "COMPLETED", todoId, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Todo");

        return app;
    }
}
