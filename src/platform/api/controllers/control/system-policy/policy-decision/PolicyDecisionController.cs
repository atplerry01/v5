using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemPolicy.PolicyDecision;

[Authorize]
[ApiController]
[Route("api/control/policy-decision")]
[ApiExplorerSettings(GroupName = "control.system-policy.policy-decision")]
public sealed class PolicyDecisionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-policy", "policy-decision");

    public PolicyDecisionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("record")]
    public Task<IActionResult> Record([FromBody] ApiRequest<RecordPolicyDecisionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordPolicyDecisionCommand(
            p.DecisionId, p.PolicyDefinitionId, p.SubjectId, p.Action, p.Resource, p.Outcome, p.DecidedAt);
        return Dispatch(cmd, Route, "policy_decision_recorded", "control.system-policy.policy-decision.record_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyDecisionReadModel>(
            id,
            "projection_control_system_policy_policy_decision",
            "policy_decision_read_model",
            "control.system-policy.policy-decision.not_found",
            ct);
}

public sealed record RecordPolicyDecisionRequestModel(
    Guid DecisionId,
    string PolicyDefinitionId,
    string SubjectId,
    string Action,
    string Resource,
    string Outcome,
    DateTimeOffset DecidedAt);
