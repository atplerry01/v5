using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Command.CommandRouting;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Command.CommandRouting;

[Authorize]
[ApiController]
[Route("api/platform/command/command-routing")]
[ApiExplorerSettings(GroupName = "platform.command.command_routing")]
public sealed class CommandRoutingController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "command", "command-routing");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public CommandRoutingController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterCommandRoutingRuleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:command:command-routing:{p.CommandDefinitionId}:{p.HandlerDomain}");
        var cmd = new RegisterCommandRoutingRuleCommand(id, p.CommandDefinitionId,
            p.HandlerClassification, p.HandlerContext, p.HandlerDomain, _clock.UtcNow);
        return Dispatch(cmd, "command_routing_rule_registered", "platform.command.command_routing.register_failed", ct);
    }

    [HttpPost("remove")]
    public Task<IActionResult> Remove([FromBody] ApiRequest<RemoveCommandRoutingRuleRequestModel> request, CancellationToken ct)
    {
        var cmd = new RemoveCommandRoutingRuleCommand(request.Data.CommandRoutingRuleId, _clock.UtcNow);
        return Dispatch(cmd, "command_routing_rule_removed", "platform.command.command_routing.remove_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterCommandRoutingRuleRequestModel(
    Guid CommandDefinitionId,
    string HandlerClassification,
    string HandlerContext,
    string HandlerDomain);

public sealed record RemoveCommandRoutingRuleRequestModel(Guid CommandRoutingRuleId);
