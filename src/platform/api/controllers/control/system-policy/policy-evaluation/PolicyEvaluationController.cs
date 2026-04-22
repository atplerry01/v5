using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemPolicy.PolicyEvaluation;

[Authorize]
[ApiController]
[Route("api/control/policy-evaluation")]
[ApiExplorerSettings(GroupName = "control.system-policy.policy-evaluation")]
public sealed class PolicyEvaluationController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-policy", "policy-evaluation");

    public PolicyEvaluationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("record")]
    public Task<IActionResult> Record([FromBody] ApiRequest<RecordPolicyEvaluationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordPolicyEvaluationCommand(p.EvaluationId, p.PolicyId, p.ActorId, p.Action, p.CorrelationId);
        return Dispatch(cmd, Route, "policy_evaluation_recorded", "control.system-policy.policy-evaluation.record_failed", ct);
    }

    [HttpPost("issue-verdict")]
    public Task<IActionResult> IssueVerdict([FromBody] ApiRequest<IssuePolicyEvaluationVerdictRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new IssuePolicyEvaluationVerdictCommand(p.EvaluationId, p.Outcome, p.DecisionHash);
        return Dispatch(cmd, Route, "policy_evaluation_verdict_issued", "control.system-policy.policy-evaluation.issue_verdict_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyEvaluationReadModel>(
            id,
            "projection_control_system_policy_policy_evaluation",
            "policy_evaluation_read_model",
            "control.system-policy.policy-evaluation.not_found",
            ct);
}

public sealed record RecordPolicyEvaluationRequestModel(
    Guid EvaluationId,
    string PolicyId,
    string ActorId,
    string Action,
    string CorrelationId);

public sealed record IssuePolicyEvaluationVerdictRequestModel(
    Guid EvaluationId,
    string Outcome,
    string DecisionHash);
