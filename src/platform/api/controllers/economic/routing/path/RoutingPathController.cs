using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Routing.Path;

[Authorize]
[ApiController]
[Route("api/routing/path")]
[ApiExplorerSettings(GroupName = "economic.routing.path")]
public sealed class RoutingPathController : ControllerBase
{
    private static readonly DomainRoute PathRoute = new("economic", "routing", "path");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public RoutingPathController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineRoutingPathRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var pathId = _idGenerator.Generate(
            $"economic:routing:path:{p.PathType}:{p.Conditions}:{p.Priority}");
        var command = new DefineRoutingPathCommand(pathId, p.PathType, p.Conditions, p.Priority);
        return Dispatch(command, "routing_path_defined", "economic.routing.path.define_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<RoutingPathIdRequestModel> request, CancellationToken ct)
    {
        var command = new ActivateRoutingPathCommand(request.Data.PathId);
        return Dispatch(command, "routing_path_activated", "economic.routing.path.activate_failed", ct);
    }

    [HttpPost("disable")]
    public Task<IActionResult> Disable([FromBody] ApiRequest<RoutingPathIdRequestModel> request, CancellationToken ct)
    {
        var command = new DisableRoutingPathCommand(request.Data.PathId);
        return Dispatch(command, "routing_path_disabled", "economic.routing.path.disable_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, PathRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow));
    }
}

public sealed record DefineRoutingPathRequestModel(
    string PathType,
    string Conditions,
    int Priority);

public sealed record RoutingPathIdRequestModel(Guid PathId);
