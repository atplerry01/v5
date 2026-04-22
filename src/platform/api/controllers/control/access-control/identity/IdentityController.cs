using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.AccessControl.Identity;

[Authorize]
[ApiController]
[Route("api/control/identity")]
[ApiExplorerSettings(GroupName = "control.access-control.identity")]
public sealed class IdentityController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "access-control", "identity");

    public IdentityController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterIdentityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RegisterIdentityCommand(p.IdentityId, p.Name, p.Kind);
        return Dispatch(cmd, Route, "identity_registered", "control.access-control.identity.register_failed", ct);
    }

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendIdentityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SuspendIdentityCommand(p.IdentityId, p.Reason);
        return Dispatch(cmd, Route, "identity_suspended", "control.access-control.identity.suspend_failed", ct);
    }

    [HttpPost("deactivate")]
    public Task<IActionResult> Deactivate([FromBody] ApiRequest<IdentityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeactivateIdentityCommand(request.Data.IdentityId);
        return Dispatch(cmd, Route, "identity_deactivated", "control.access-control.identity.deactivate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<IdentityReadModel>(
            id,
            "projection_control_access_control_identity",
            "identity_read_model",
            "control.access-control.identity.not_found",
            ct);
}

public sealed record RegisterIdentityRequestModel(Guid IdentityId, string Name, string Kind);
public sealed record SuspendIdentityRequestModel(Guid IdentityId, string Reason);
public sealed record IdentityIdRequestModel(Guid IdentityId);
