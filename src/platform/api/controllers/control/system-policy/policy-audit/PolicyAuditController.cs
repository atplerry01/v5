using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemPolicy.PolicyAudit;

[Authorize]
[ApiController]
[Route("api/control/policy-audit")]
[ApiExplorerSettings(GroupName = "control.system-policy.policy-audit")]
public sealed class PolicyAuditController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-policy", "policy-audit");

    public PolicyAuditController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("record")]
    public Task<IActionResult> Record([FromBody] ApiRequest<RecordPolicyAuditEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordPolicyAuditEntryCommand(
            p.AuditId, p.PolicyId, p.ActorId, p.Action, p.Category, p.DecisionHash, p.CorrelationId, p.OccurredAt);
        return Dispatch(cmd, Route, "policy_audit_entry_recorded", "control.system-policy.policy-audit.record_failed", ct);
    }

    [HttpPost("review")]
    public Task<IActionResult> Review([FromBody] ApiRequest<ReviewPolicyAuditEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReviewPolicyAuditEntryCommand(p.AuditId, p.ReviewerId, p.Reason);
        return Dispatch(cmd, Route, "policy_audit_entry_reviewed", "control.system-policy.policy-audit.review_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyAuditReadModel>(
            id,
            "projection_control_system_policy_policy_audit",
            "policy_audit_read_model",
            "control.system-policy.policy-audit.not_found",
            ct);
}

public sealed record RecordPolicyAuditEntryRequestModel(
    Guid AuditId,
    string PolicyId,
    string ActorId,
    string Action,
    string Category,
    string DecisionHash,
    string CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record ReviewPolicyAuditEntryRequestModel(
    Guid AuditId,
    string ReviewerId,
    string Reason);
