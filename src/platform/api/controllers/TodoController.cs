using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Api.Controllers;

[ApiController]
[Route("api/todo")]
[ApiExplorerSettings(GroupName = "operational.sandbox.todo")]
public sealed class TodoController : ControllerBase
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IRedisClient _redis;

    public TodoController(ISystemIntentDispatcher dispatcher, IRedisClient redis)
    {
        _dispatcher = dispatcher;
        _redis = redis;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTodoCommand cmd)
    {
        var result = await _dispatcher.DispatchAsync(cmd);
        return result.IsSuccess ? Ok(new { status = "created" }) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var readModel = await _redis.GetAsync<TodoReadModel>($"todo:{id}");
        if (readModel is null) return NotFound();

        return Ok(readModel);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateTodoCommand cmd)
    {
        var result = await _dispatcher.DispatchAsync(cmd);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteTodoCommand cmd)
    {
        var result = await _dispatcher.DispatchAsync(cmd);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }
}
