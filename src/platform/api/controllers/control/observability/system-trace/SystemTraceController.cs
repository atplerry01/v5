using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Observability.SystemTrace;

[Authorize]
[ApiController]
[Route("api/control/system-trace")]
[ApiExplorerSettings(GroupName = "control.observability.system-trace")]
public sealed class SystemTraceController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "observability", "system-trace");

    public SystemTraceController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartSystemTraceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new StartSystemTraceCommand(p.SpanId, p.TraceId, p.OperationName, p.StartedAt, p.ParentSpanId);
        return Dispatch(cmd, Route, "system_trace_started", "control.observability.system-trace.start_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteSystemTraceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteSystemTraceCommand(p.SpanId, p.CompletedAt, p.Status);
        return Dispatch(cmd, Route, "system_trace_completed", "control.observability.system-trace.complete_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemTraceReadModel>(
            id,
            "projection_control_observability_system_trace",
            "system_trace_read_model",
            "control.observability.system-trace.not_found",
            ct);
}

public sealed record StartSystemTraceRequestModel(
    Guid SpanId,
    string TraceId,
    string OperationName,
    DateTimeOffset StartedAt,
    string? ParentSpanId = null);

public sealed record CompleteSystemTraceRequestModel(
    Guid SpanId,
    DateTimeOffset CompletedAt,
    string Status);
