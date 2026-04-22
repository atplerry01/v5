using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Audit.AuditEvent;

[Authorize]
[ApiController]
[Route("api/control/audit-event")]
[ApiExplorerSettings(GroupName = "control.audit.audit-event")]
public sealed class AuditEventController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "audit", "audit-event");

    public AuditEventController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("capture")]
    public Task<IActionResult> Capture([FromBody] ApiRequest<CaptureAuditEventRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CaptureAuditEventCommand(p.AuditEventId, p.ActorId, p.Action, p.Kind, p.CorrelationId, p.OccurredAt);
        return Dispatch(cmd, Route, "audit_event_captured", "control.audit.audit-event.capture_failed", ct);
    }

    [HttpPost("seal")]
    public Task<IActionResult> Seal([FromBody] ApiRequest<SealAuditEventRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SealAuditEventCommand(p.AuditEventId, p.IntegrityHash);
        return Dispatch(cmd, Route, "audit_event_sealed", "control.audit.audit-event.seal_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AuditEventReadModel>(
            id,
            "projection_control_audit_audit_event",
            "audit_event_read_model",
            "control.audit.audit-event.not_found",
            ct);
}

public sealed record CaptureAuditEventRequestModel(Guid AuditEventId, string ActorId, string Action, string Kind, string CorrelationId, DateTimeOffset OccurredAt);
public sealed record SealAuditEventRequestModel(Guid AuditEventId, string IntegrityHash);
