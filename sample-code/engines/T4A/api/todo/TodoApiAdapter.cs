using System.Text.Json;
using Whycespace.Shared.Contracts.Command;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T4A.Api.Todo;

public sealed class TodoApiAdapter
{
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public TodoApiAdapter(IClock clock, IIdGenerator idGen)
    {
        _clock = clock;
        _idGen = idGen;
    }

    public CommandEnvelope AdaptCreateTodo(JsonElement body)
    {
        var todoId = body.TryGetProperty("todo_id", out var id)
            ? id.GetGuid()
            : _idGen.DeterministicGuid($"CreateTodo:{body.GetProperty("title").GetString()}");

        var payload = new
        {
            TodoId = todoId,
            Title = body.GetProperty("title").GetString()!,
            Description = body.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
            AssignedTo = body.TryGetProperty("assigned_to", out var assignee) ? assignee.GetString() ?? "" : "",
            Priority = body.TryGetProperty("priority", out var prio) ? prio.GetInt32() : 0
        };

        return new CommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"CreateTodo:CommandId:{todoId}"),
            CommandType = "CreateTodo",
            Payload = payload,
            CorrelationId = _idGen.DeterministicGuid($"CreateTodo:CorrelationId:{todoId}").ToString(),
            Timestamp = _clock.UtcNowOffset,
            Metadata = new CommandMetadata()
        };
    }

    public CommandEnvelope AdaptCompleteTodo(Guid todoId)
    {
        var payload = new { TodoId = todoId };

        return new CommandEnvelope
        {
            CommandId = _idGen.DeterministicGuid($"CompleteTodo:CommandId:{todoId}"),
            CommandType = "CompleteTodo",
            Payload = payload,
            CorrelationId = _idGen.DeterministicGuid($"CompleteTodo:CorrelationId:{todoId}").ToString(),
            Timestamp = _clock.UtcNowOffset,
            Metadata = new CommandMetadata()
        };
    }
}
