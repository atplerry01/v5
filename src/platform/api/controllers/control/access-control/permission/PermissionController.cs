using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.AccessControl.Permission;

[Authorize]
[ApiController]
[Route("api/control/permission")]
[ApiExplorerSettings(GroupName = "control.access-control.permission")]
public sealed class PermissionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "access-control", "permission");

    public PermissionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefinePermissionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefinePermissionCommand(p.PermissionId, p.Name, p.ResourceScope, p.Actions);
        return Dispatch(cmd, Route, "permission_defined", "control.access-control.permission.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<PermissionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecatePermissionCommand(request.Data.PermissionId);
        return Dispatch(cmd, Route, "permission_deprecated", "control.access-control.permission.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PermissionReadModel>(
            id,
            "projection_control_access_control_permission",
            "permission_read_model",
            "control.access-control.permission.not_found",
            ct);
}

public sealed record DefinePermissionRequestModel(Guid PermissionId, string Name, string ResourceScope, string Actions);
public sealed record PermissionIdRequestModel(Guid PermissionId);
