using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Audit.AuditLog;

[Authorize]
[ApiController]
[Route("api/control/audit-log")]
[ApiExplorerSettings(GroupName = "control.audit.audit-log")]
public sealed class AuditLogController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "audit", "audit-log");

    public AuditLogController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("record")]
    public Task<IActionResult> Record([FromBody] ApiRequest<RecordAuditLogEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordAuditLogEntryCommand(p.AuditLogId, p.ActorId, p.Action, p.Resource, p.Classification, p.OccurredAt, p.DecisionId);
        return Dispatch(cmd, Route, "audit_log_entry_recorded", "control.audit.audit-log.record_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AuditLogReadModel>(
            id,
            "projection_control_audit_audit_log",
            "audit_log_read_model",
            "control.audit.audit-log.not_found",
            ct);
}

public sealed record RecordAuditLogEntryRequestModel(
    Guid AuditLogId, string ActorId, string Action, string Resource, string Classification, DateTimeOffset OccurredAt, string? DecisionId = null);
