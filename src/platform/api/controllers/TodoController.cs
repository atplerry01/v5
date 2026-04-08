using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Api.Controllers;

[ApiController]
[Route("api/todo")]
[ApiExplorerSettings(GroupName = "operational.sandbox.todo")]
public sealed class TodoController : ControllerBase
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IRedisClient _redis;
    private readonly string _projectionsConnectionString;

    public TodoController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IRedisClient redis,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _redis = redis;
        // phase1.5-S1 (CFG-R3, CFG-R4): no fallback — env var is required.
        // Indexer access replaced with GetValue<T>() to match the canonical pattern.
        _projectionsConnectionString = configuration.GetValue<string>("Projections__ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections__ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest request)
    {
        // phase1-gate-S1: route through dispatcher so the policy envelope,
        // guards, outbox, and chain anchor all run — same path as Update/Complete.
        // phase1.6-S1.1 (DET-SEED-DERIVATION-01): seed contains only stable
        // request coordinates — no clock, no random. Two creates with the same
        // (user, title) collapse to the same aggregate id, making the endpoint
        // idempotent under retry. Callers that need distinct todos with the
        // same title must vary the title (the alternative — accepting a client
        // idempotency key — is the cleaner long-term API but out of scope for
        // S1.1, which is constrained to seed-derivation only).
        var aggregateId = _idGenerator.Generate(
            $"todo:create:{request.UserId}:{request.Title}");
        var cmd = new CreateTodoCommand(aggregateId, request.Title);
        var result = await _dispatcher.DispatchAsync(cmd, TodoRoute);
        return result.IsSuccess
            ? Ok(new { status = "created", todoId = aggregateId, correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            SELECT aggregate_id, current_version, last_event_type, state
            FROM projection_operational_sandbox_todo.todo_read_model
            WHERE aggregate_id = @id
            LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        var found = await reader.ReadAsync();
        if (!found) return NotFound();

        var lastEventType = reader.GetString(2);
        var stateJson = reader.GetString(3);

        string title = string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(stateJson);
            if (doc.RootElement.TryGetProperty("Title", out var t)) title = t.GetString() ?? string.Empty;
        }
        catch { }

        var status = lastEventType == "TodoCompletedEvent" ? "completed" : "active";

        return Ok(new TodoReadModel
        {
            Id = id,
            Title = title,
            IsCompleted = status == "completed",
            Status = status
        });
    }

    private static readonly DomainRoute TodoRoute = new("operational", "sandbox", "todo");

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateTodoCommand cmd)
    {
        var result = await _dispatcher.DispatchAsync(cmd, TodoRoute);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteTodoCommand cmd)
    {
        var result = await _dispatcher.DispatchAsync(cmd, TodoRoute);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }
}

public sealed record CreateTodoRequest(string Title, string? Description, string UserId);
