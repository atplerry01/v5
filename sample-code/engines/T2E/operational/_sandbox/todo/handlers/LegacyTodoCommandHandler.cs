using System.Text.Json;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T2E.Operational._Sandbox.Todo;

/// <summary>
/// Legacy todo command handler. Publishes domain events via IEventPublisher
/// (transport-agnostic) — no direct Kafka dependency.
/// </summary>
public sealed class LegacyTodoCommandHandler
{
    private readonly ILogger<LegacyTodoCommandHandler> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public LegacyTodoCommandHandler(ILogger<LegacyTodoCommandHandler> logger, IEventPublisher eventPublisher, IIdGenerator idGenerator, IClock clock)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    public Task<CommandHandlerResult> HandleAsync(
        string commandType, JsonElement payload, string correlationId, CancellationToken ct)
    {
        _logger.LogInformation(
            "Handling command {CommandType} correlation={CorrelationId}",
            commandType, correlationId);

        return commandType switch
        {
            "create_todo" => HandleCreateTodoAsync(payload, correlationId, ct),
            "complete_todo" => HandleCompleteTodoAsync(payload, correlationId, ct),
            _ => Task.FromResult(CommandHandlerResult.Fail(
                "UNKNOWN_COMMAND", $"Unrecognized command type: {commandType}"))
        };
    }

    private async Task<CommandHandlerResult> HandleCreateTodoAsync(
        JsonElement payload, string correlationId, CancellationToken ct)
    {
        var title = payload.TryGetProperty("title", out var t) ? t.GetString() : null;

        if (string.IsNullOrWhiteSpace(title))
            return CommandHandlerResult.Fail("VALIDATION", "Title is required.");

        var todoId = payload.TryGetProperty("todo_id", out var idProp)
            ? idProp.GetString() ?? _idGenerator.DeterministicGuid("todo", correlationId, "create").ToString()
            : _idGenerator.DeterministicGuid("todo", correlationId, "create").ToString();

        var priority = payload.TryGetProperty("priority", out var p) ? p.GetInt32() : 0;
        var assignedTo = payload.TryGetProperty("assigned_to", out var a) ? a.GetString() : null;

        var eventId = _idGenerator.DeterministicGuid("todo", todoId, correlationId, "created");

        await _eventPublisher.PublishAsync(new RuntimeEvent
        {
            EventId = eventId,
                AggregateId = Guid.Empty,
            AggregateType = "todo",
            EventType = "whyce.operational.sandbox.todo.created",
            CorrelationId = correlationId,
            Payload = new
            {
                todo_id = Guid.Parse(todoId),
                title,
                priority,
                assigned_to = assignedTo,
                created_at = _clock.UtcNowOffset
            },
            Timestamp = _clock.UtcNowOffset,
            Cluster = "operational",
            SubCluster = "sandbox",
            Context = "todo"
        }, ct);

        _logger.LogInformation("Todo created: {TodoId} title={Title} correlation={CorrelationId}",
            todoId, title, correlationId);

        return CommandHandlerResult.Ok(todoId);
    }

    private async Task<CommandHandlerResult> HandleCompleteTodoAsync(
        JsonElement payload, string correlationId, CancellationToken ct)
    {
        var todoId = payload.TryGetProperty("todo_id", out var id) ? id.GetString() : null;

        if (string.IsNullOrWhiteSpace(todoId))
            return CommandHandlerResult.Fail("VALIDATION", "Todo ID is required.");

        var eventId = _idGenerator.DeterministicGuid("todo", todoId!, correlationId, "completed");

        await _eventPublisher.PublishAsync(new RuntimeEvent
        {
            EventId = eventId,
                AggregateId = Guid.Empty,
            AggregateType = "todo",
            EventType = "whyce.operational.sandbox.todo.completed",
            CorrelationId = correlationId,
            Payload = new
            {
                todo_id = Guid.Parse(todoId)
            },
            Timestamp = _clock.UtcNowOffset,
            Cluster = "operational",
            SubCluster = "sandbox",
            Context = "todo"
        }, ct);

        _logger.LogInformation("Todo completed: {TodoId} correlation={CorrelationId}", todoId, correlationId);

        return CommandHandlerResult.Ok(todoId);
    }
}

public sealed record CommandHandlerResult(bool Success, string? ErrorCode = null, string? ErrorMessage = null, string? Data = null)
{
    public static CommandHandlerResult Ok(string? data = null) => new(true, Data: data);
    public static CommandHandlerResult Fail(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}
