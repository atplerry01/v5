using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Routing.Execution;

[Authorize]
[ApiController]
[Route("api/routing/execution")]
[ApiExplorerSettings(GroupName = "economic.routing.execution")]
public sealed class RoutingExecutionController : ControllerBase
{
    private static readonly DomainRoute ExecutionRoute = new("economic", "routing", "execution");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public RoutingExecutionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartExecutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var executionId = _idGenerator.Generate(
            $"economic:routing:execution:{p.PathId}:{_clock.UtcNow.ToUnixTimeMilliseconds()}");
        var command = new StartExecutionCommand(executionId, p.PathId);
        return Dispatch(command, "routing_execution_started", "economic.routing.execution.start_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<RoutingExecutionIdRequestModel> request, CancellationToken ct)
    {
        var command = new CompleteExecutionCommand(request.Data.ExecutionId);
        return Dispatch(command, "routing_execution_completed", "economic.routing.execution.complete_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailExecutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var command = new FailExecutionCommand(p.ExecutionId, p.Reason);
        return Dispatch(command, "routing_execution_failed", "economic.routing.execution.fail_failed", ct);
    }

    [HttpPost("abort")]
    public Task<IActionResult> Abort([FromBody] ApiRequest<AbortExecutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var command = new AbortExecutionCommand(p.ExecutionId, p.Reason);
        return Dispatch(command, "routing_execution_aborted", "economic.routing.execution.abort_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ExecutionRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow));
    }
}

public sealed record StartExecutionRequestModel(Guid PathId);

public sealed record RoutingExecutionIdRequestModel(Guid ExecutionId);

public sealed record FailExecutionRequestModel(Guid ExecutionId, string Reason);

public sealed record AbortExecutionRequestModel(Guid ExecutionId, string Reason);
