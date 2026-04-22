using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemPolicy.PolicyDefinition;

[Authorize]
[ApiController]
[Route("api/control/policy-definition")]
[ApiExplorerSettings(GroupName = "control.system-policy.policy-definition")]
public sealed class PolicyDefinitionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-policy", "policy-definition");

    public PolicyDefinitionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefinePolicyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefinePolicyCommand(p.PolicyId, p.Name, p.ScopeClassification, p.ScopeActionMask, p.ScopeContext);
        return Dispatch(cmd, Route, "policy_defined", "control.system-policy.policy-definition.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<PolicyIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecatePolicyCommand(request.Data.PolicyId);
        return Dispatch(cmd, Route, "policy_deprecated", "control.system-policy.policy-definition.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyDefinitionReadModel>(
            id,
            "projection_control_system_policy_policy_definition",
            "policy_definition_read_model",
            "control.system-policy.policy-definition.not_found",
            ct);
}

public sealed record DefinePolicyRequestModel(
    Guid PolicyId,
    string Name,
    string ScopeClassification,
    string ScopeActionMask,
    string? ScopeContext = null);

public sealed record PolicyIdRequestModel(Guid PolicyId);
