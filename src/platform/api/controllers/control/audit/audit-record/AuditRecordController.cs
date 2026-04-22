using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Audit.AuditRecord;

[Authorize]
[ApiController]
[Route("api/control/audit-record")]
[ApiExplorerSettings(GroupName = "control.audit.audit-record")]
public sealed class AuditRecordController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "audit", "audit-record");

    public AuditRecordController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("raise")]
    public Task<IActionResult> Raise([FromBody] ApiRequest<RaiseAuditRecordRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RaiseAuditRecordCommand(p.RecordId, p.AuditLogEntryIds, p.Description, p.Severity, p.RaisedAt);
        return Dispatch(cmd, Route, "audit_record_raised", "control.audit.audit-record.raise_failed", ct);
    }

    [HttpPost("resolve")]
    public Task<IActionResult> Resolve([FromBody] ApiRequest<ResolveAuditRecordRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ResolveAuditRecordCommand(p.RecordId, p.ResolvedAt);
        return Dispatch(cmd, Route, "audit_record_resolved", "control.audit.audit-record.resolve_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AuditRecordReadModel>(
            id,
            "projection_control_audit_audit_record",
            "audit_record_read_model",
            "control.audit.audit-record.not_found",
            ct);
}

public sealed record RaiseAuditRecordRequestModel(
    Guid RecordId, IReadOnlyList<string> AuditLogEntryIds, string Description, string Severity, DateTimeOffset RaisedAt);
public sealed record ResolveAuditRecordRequestModel(Guid RecordId, DateTimeOffset ResolvedAt);
