using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Audit.AuditTrace;

[Authorize]
[ApiController]
[Route("api/control/audit-trace")]
[ApiExplorerSettings(GroupName = "control.audit.audit-trace")]
public sealed class AuditTraceController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "audit", "audit-trace");

    public AuditTraceController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("open")]
    public Task<IActionResult> Open([FromBody] ApiRequest<OpenAuditTraceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new OpenAuditTraceCommand(p.TraceId, p.CorrelationId, p.OpenedAt);
        return Dispatch(cmd, Route, "audit_trace_opened", "control.audit.audit-trace.open_failed", ct);
    }

    [HttpPost("link-event")]
    public Task<IActionResult> LinkEvent([FromBody] ApiRequest<LinkAuditTraceEventRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new LinkAuditTraceEventCommand(p.TraceId, p.AuditEventId);
        return Dispatch(cmd, Route, "audit_trace_event_linked", "control.audit.audit-trace.link_event_failed", ct);
    }

    [HttpPost("close")]
    public Task<IActionResult> Close([FromBody] ApiRequest<CloseAuditTraceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CloseAuditTraceCommand(p.TraceId, p.ClosedAt);
        return Dispatch(cmd, Route, "audit_trace_closed", "control.audit.audit-trace.close_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AuditTraceReadModel>(
            id,
            "projection_control_audit_audit_trace",
            "audit_trace_read_model",
            "control.audit.audit-trace.not_found",
            ct);
}

public sealed record OpenAuditTraceRequestModel(Guid TraceId, string CorrelationId, DateTimeOffset OpenedAt);
public sealed record LinkAuditTraceEventRequestModel(Guid TraceId, string AuditEventId);
public sealed record CloseAuditTraceRequestModel(Guid TraceId, DateTimeOffset ClosedAt);
