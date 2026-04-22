using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Event.EventStream;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Event.EventStream;

[Authorize]
[ApiController]
[Route("api/platform/event/event-stream")]
[ApiExplorerSettings(GroupName = "platform.event.event_stream")]
public sealed class EventStreamController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "event", "event-stream");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public EventStreamController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("declare")]
    public Task<IActionResult> Declare([FromBody] ApiRequest<DeclareEventStreamRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:event:event-stream:{p.SourceClassification}:{p.SourceContext}:{p.SourceDomain}");
        var cmd = new DeclareEventStreamCommand(id, p.SourceClassification, p.SourceContext, p.SourceDomain,
            p.IncludedEventTypes, p.OrderingGuarantee, _clock.UtcNow);
        return Dispatch(cmd, "event_stream_declared", "platform.event.event_stream.declare_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DeclareEventStreamRequestModel(
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    IReadOnlyList<string> IncludedEventTypes,
    string OrderingGuarantee);
