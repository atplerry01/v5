using Microsoft.AspNetCore.Mvc;
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

    public TodoController(
        ITodoIntentHandler intentHandler,
        ISystemIntentDispatcher dispatcher,
        IRedisClient redis)
    {
        _intentHandler = intentHandler;
        _dispatcher = dispatcher;
        _redis = redis;
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
        var readModel = await _redis.GetAsync<TodoReadModel>($"todo:{id}");
        if (readModel is null) return NotFound();

        return Ok(readModel);
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
