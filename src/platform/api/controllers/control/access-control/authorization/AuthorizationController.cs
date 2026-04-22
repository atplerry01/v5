using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.AccessControl.Authorization;

[Authorize]
[ApiController]
[Route("api/control/authorization")]
[ApiExplorerSettings(GroupName = "control.access-control.authorization")]
public sealed class AuthorizationController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "access-control", "authorization");

    public AuthorizationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("grant")]
    public Task<IActionResult> Grant([FromBody] ApiRequest<GrantAuthorizationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new GrantAuthorizationCommand(p.AuthorizationId, p.SubjectId, p.RoleIds, p.ValidFrom, p.ValidTo);
        return Dispatch(cmd, Route, "authorization_granted", "control.access-control.authorization.grant_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<AuthorizationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeAuthorizationCommand(request.Data.AuthorizationId);
        return Dispatch(cmd, Route, "authorization_revoked", "control.access-control.authorization.revoke_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<AuthorizationReadModel>(
            id,
            "projection_control_access_control_authorization",
            "authorization_read_model",
            "control.access-control.authorization.not_found",
            ct);
}

public sealed record GrantAuthorizationRequestModel(
    Guid AuthorizationId,
    string SubjectId,
    IReadOnlyList<string> RoleIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo = null);

public sealed record AuthorizationIdRequestModel(Guid AuthorizationId);
