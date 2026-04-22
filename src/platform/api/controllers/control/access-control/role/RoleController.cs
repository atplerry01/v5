using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.AccessControl.Role;

[Authorize]
[ApiController]
[Route("api/control/role")]
[ApiExplorerSettings(GroupName = "control.access-control.role")]
public sealed class RoleController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "access-control", "role");

    public RoleController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineRoleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineRoleCommand(p.RoleId, p.Name, p.PermissionIds, p.ParentRoleId);
        return Dispatch(cmd, Route, "role_defined", "control.access-control.role.define_failed", ct);
    }

    [HttpPost("add-permission")]
    public Task<IActionResult> AddPermission([FromBody] ApiRequest<AddRolePermissionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddRolePermissionCommand(p.RoleId, p.PermissionId);
        return Dispatch(cmd, Route, "role_permission_added", "control.access-control.role.add_permission_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<RoleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateRoleCommand(request.Data.RoleId);
        return Dispatch(cmd, Route, "role_deprecated", "control.access-control.role.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<RoleReadModel>(
            id,
            "projection_control_access_control_role",
            "role_read_model",
            "control.access-control.role.not_found",
            ct);
}

public sealed record DefineRoleRequestModel(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionIds,
    string? ParentRoleId = null);

public sealed record AddRolePermissionRequestModel(Guid RoleId, string PermissionId);
public sealed record RoleIdRequestModel(Guid RoleId);
