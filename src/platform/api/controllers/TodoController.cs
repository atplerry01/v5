using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;

namespace Whyce.Platform.Api.Controllers;

[ApiController]
[Route("api/todo")]
[ApiExplorerSettings(GroupName = "operational.sandbox.todo")]
public sealed class TodoController : ControllerBase
{
    private readonly ITodoIntentHandler _intentHandler;
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IRedisClient _redis;
    private readonly string _projectionsConnectionString;

    public TodoController(
        ITodoIntentHandler intentHandler,
        ISystemIntentDispatcher dispatcher,
        IRedisClient redis,
        IConfiguration configuration)
    {
        _intentHandler = intentHandler;
        _dispatcher = dispatcher;
        _redis = redis;
        _projectionsConnectionString = configuration["Projections__ConnectionString"]
            ?? "Host=localhost;Port=5434;Database=whyce_projections;Username=whyce;Password=whyce";
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest request)
    {
        var intent = new CreateTodoIntent(request.Title, request.Description ?? string.Empty, request.UserId);
        var result = await _intentHandler.HandleAsync(intent);
        return result.Success
            ? Ok(new { status = result.Status, todoId = result.TodoId })
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        Console.WriteLine($"[GET] Querying projection for: {id}");

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
        Console.WriteLine($"[GET] Row found: {found}");
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
