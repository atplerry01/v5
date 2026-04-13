using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Operational.Sandbox.Todo;

// WP-1: All operational endpoints require authenticated identity.
// No request reaches execution without a valid JWT Bearer token.
[Authorize]
[ApiController]
[Route("api/todo")]
[ApiExplorerSettings(GroupName = "operational.sandbox.todo")]
public sealed class TodoController : ControllerBase
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IRedisClient _redis;
    private readonly IClock _clock;
    private readonly ILogger<TodoController> _logger;
    private readonly string _projectionsConnectionString;

    public TodoController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IRedisClient redis,
        IClock clock,
        IConfiguration configuration,
        ILogger<TodoController> logger)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _redis = redis;
        _clock = clock;
        _logger = logger;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ApiRequest<CreateTodoRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var aggregateId = _idGenerator.Generate(
            $"todo:create:{payload.UserId}:{payload.Title}");
        var cmd = new CreateTodoCommand(aggregateId, payload.Title);
        var result = await _dispatcher.DispatchAsync(cmd, TodoRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CreateTodoResponseModel(aggregateId), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.todo.create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT aggregate_id, current_version, last_event_type, state
            FROM projection_operational_sandbox_todo.todo_read_model
            WHERE aggregate_id = @id
            LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var found = await reader.ReadAsync(cancellationToken);
        if (!found)
            return NotFound(ApiResponse.Fail(
                "operational.sandbox.todo.not_found",
                $"Todo {id} not found.",
                _clock.UtcNow));

        var lastEventType = reader.GetString(2);
        var stateJson = reader.GetString(3);

        string title = string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(stateJson);
            if (doc.RootElement.TryGetProperty("Title", out var t)) title = t.GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in TodoController operation");
            throw;
        }

        var status = lastEventType == "TodoCompletedEvent" ? "completed" : "active";

        return Ok(ApiResponse.Ok(new TodoReadModel
        {
            Id = id,
            Title = title,
            IsCompleted = status == "completed",
            Status = status
        }, _clock.UtcNow));
    }

    private static readonly DomainRoute TodoRoute = new("operational", "sandbox", "todo");

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] ApiRequest<UpdateTodoCommand> request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.DispatchAsync(request.Data, TodoRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("updated"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.todo.update_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] ApiRequest<CompleteTodoCommand> request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.DispatchAsync(request.Data, TodoRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("completed"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.todo.complete_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }
}

public sealed record CreateTodoRequestModel(string Title, string? Description, string UserId);
public sealed record CreateTodoResponseModel(Guid TodoId);
