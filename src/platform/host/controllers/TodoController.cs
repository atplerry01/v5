using Microsoft.AspNetCore.Mvc;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Api.Controllers;

[ApiController]
[Route("api/todo")]
public sealed class TodoController : ControllerBase
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public TodoController(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTodoCommand cmd)
    {
        var result = await _dispatcher.DispatchAsync(cmd);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
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
