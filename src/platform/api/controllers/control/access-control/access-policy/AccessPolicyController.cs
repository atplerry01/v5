using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.AccessControl.AccessPolicy;

[Authorize]
[ApiController]
[Route("api/control/access-policy")]
[ApiExplorerSettings(GroupName = "control.access-control.access-policy")]
public sealed class AccessPolicyController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "access-control", "access-policy");

    public AccessPolicyController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineAccessPolicyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineAccessPolicyCommand(p.PolicyId, p.Name, p.Scope, p.AllowedRoleIds);
        return Dispatch(cmd, Route, "access_policy_defined", "control.access-control.access-policy.define_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<AccessPolicyIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateAccessPolicyCommand(request.Data.PolicyId);
        return Dispatch(cmd, Route, "access_policy_activated", "control.access-control.access-policy.activate_failed", ct);
    }

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<AccessPolicyIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RetireAccessPolicyCommand(request.Data.PolicyId);
        return Dispatch(cmd, Route, "access_policy_retired", "control.access-control.access-policy.retire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AccessPolicyReadModel>(
            id,
            "projection_control_access_control_access_policy",
            "access_policy_read_model",
            "control.access-control.access-policy.not_found",
            ct);
}

public sealed record DefineAccessPolicyRequestModel(
    Guid PolicyId,
    string Name,
    string Scope,
    IReadOnlyList<string> AllowedRoleIds);

public sealed record AccessPolicyIdRequestModel(Guid PolicyId);
