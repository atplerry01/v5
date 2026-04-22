using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.AccessControl.Principal;

[Authorize]
[ApiController]
[Route("api/control/principal")]
[ApiExplorerSettings(GroupName = "control.access-control.principal")]
public sealed class PrincipalController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "access-control", "principal");

    public PrincipalController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterPrincipalRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RegisterPrincipalCommand(p.PrincipalId, p.Name, p.Kind, p.IdentityId);
        return Dispatch(cmd, Route, "principal_registered", "control.access-control.principal.register_failed", ct);
    }

    [HttpPost("assign-role")]
    public Task<IActionResult> AssignRole([FromBody] ApiRequest<AssignPrincipalRoleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AssignPrincipalRoleCommand(p.PrincipalId, p.RoleId);
        return Dispatch(cmd, Route, "principal_role_assigned", "control.access-control.principal.assign_role_failed", ct);
    }

    [HttpPost("deactivate")]
    public Task<IActionResult> Deactivate([FromBody] ApiRequest<PrincipalIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeactivatePrincipalCommand(request.Data.PrincipalId);
        return Dispatch(cmd, Route, "principal_deactivated", "control.access-control.principal.deactivate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PrincipalReadModel>(
            id,
            "projection_control_access_control_principal",
            "principal_read_model",
            "control.access-control.principal.not_found",
            ct);
}

public sealed record RegisterPrincipalRequestModel(Guid PrincipalId, string Name, string Kind, string IdentityId);
public sealed record AssignPrincipalRoleRequestModel(Guid PrincipalId, string RoleId);
public sealed record PrincipalIdRequestModel(Guid PrincipalId);
