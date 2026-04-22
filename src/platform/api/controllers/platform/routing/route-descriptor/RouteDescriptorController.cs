using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDescriptor;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Routing.RouteDescriptor;

[Authorize]
[ApiController]
[Route("api/platform/routing/route-descriptor")]
[ApiExplorerSettings(GroupName = "platform.routing.route_descriptor")]
public sealed class RouteDescriptorController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "routing", "route-descriptor");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public RouteDescriptorController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterRouteDescriptorRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:routing:route-descriptor:{p.SourceDomain}:{p.DestinationDomain}:{p.TransportHint}");
        var cmd = new RegisterRouteDescriptorCommand(id,
            p.SourceClassification, p.SourceContext, p.SourceDomain,
            p.DestinationClassification, p.DestinationContext, p.DestinationDomain,
            p.TransportHint, p.Priority, _clock.UtcNow);
        return Dispatch(cmd, "route_descriptor_registered", "platform.routing.route_descriptor.register_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterRouteDescriptorRequestModel(
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string DestinationClassification,
    string DestinationContext,
    string DestinationDomain,
    string TransportHint,
    int Priority);
