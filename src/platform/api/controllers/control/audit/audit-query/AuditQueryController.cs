using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Audit.AuditQuery;

[Authorize]
[ApiController]
[Route("api/control/audit-query")]
[ApiExplorerSettings(GroupName = "control.audit.audit-query")]
public sealed class AuditQueryController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "audit", "audit-query");

    public AuditQueryController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("issue")]
    public Task<IActionResult> Issue([FromBody] ApiRequest<IssueAuditQueryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new IssueAuditQueryCommand(p.QueryId, p.IssuedBy, p.TimeRangeFrom, p.TimeRangeTo, p.CorrelationFilter, p.ActorFilter);
        return Dispatch(cmd, Route, "audit_query_issued", "control.audit.audit-query.issue_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteAuditQueryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteAuditQueryCommand(p.QueryId, p.ResultCount);
        return Dispatch(cmd, Route, "audit_query_completed", "control.audit.audit-query.complete_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<QueryIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireAuditQueryCommand(request.Data.QueryId);
        return Dispatch(cmd, Route, "audit_query_expired", "control.audit.audit-query.expire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AuditQueryReadModel>(
            id,
            "projection_control_audit_audit_query",
            "audit_query_read_model",
            "control.audit.audit-query.not_found",
            ct);
}

public sealed record IssueAuditQueryRequestModel(
    Guid QueryId, string IssuedBy, DateTimeOffset TimeRangeFrom, DateTimeOffset TimeRangeTo,
    string? CorrelationFilter = null, string? ActorFilter = null);
public sealed record CompleteAuditQueryRequestModel(Guid QueryId, int ResultCount);
public sealed record QueryIdRequestModel(Guid QueryId);
