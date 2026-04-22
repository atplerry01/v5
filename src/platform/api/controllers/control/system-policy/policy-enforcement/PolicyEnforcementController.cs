using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemPolicy.PolicyEnforcement;

[Authorize]
[ApiController]
[Route("api/control/policy-enforcement")]
[ApiExplorerSettings(GroupName = "control.system-policy.policy-enforcement")]
public sealed class PolicyEnforcementController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-policy", "policy-enforcement");

    public PolicyEnforcementController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("record")]
    public Task<IActionResult> Record([FromBody] ApiRequest<RecordPolicyEnforcementRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordPolicyEnforcementCommand(
            p.EnforcementId, p.PolicyDecisionId, p.TargetId, p.Outcome, p.EnforcedAt, p.IsNoPolicyFlagAnomaly);
        return Dispatch(cmd, Route, "policy_enforcement_recorded", "control.system-policy.policy-enforcement.record_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyEnforcementReadModel>(
            id,
            "projection_control_system_policy_policy_enforcement",
            "policy_enforcement_read_model",
            "control.system-policy.policy-enforcement.not_found",
            ct);
}

public sealed record RecordPolicyEnforcementRequestModel(
    Guid EnforcementId,
    string PolicyDecisionId,
    string TargetId,
    string Outcome,
    DateTimeOffset EnforcedAt,
    bool IsNoPolicyFlagAnomaly = false);
