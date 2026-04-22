using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Event.EventSchema;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Event.EventSchema;

[Authorize]
[ApiController]
[Route("api/platform/event/event-schema")]
[ApiExplorerSettings(GroupName = "platform.event.event_schema")]
public sealed class EventSchemaController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "event", "event-schema");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public EventSchemaController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterEventSchemaRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:event:event-schema:{p.Name}:{p.Version}");
        var cmd = new RegisterEventSchemaCommand(id, p.Name, p.Version, p.CompatibilityMode, _clock.UtcNow);
        return Dispatch(cmd, "event_schema_registered", "platform.event.event_schema.register_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateEventSchemaRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateEventSchemaCommand(request.Data.EventSchemaId, _clock.UtcNow);
        return Dispatch(cmd, "event_schema_deprecated", "platform.event.event_schema.deprecate_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterEventSchemaRequestModel(string Name, string Version, string CompatibilityMode);
public sealed record DeprecateEventSchemaRequestModel(Guid EventSchemaId);
