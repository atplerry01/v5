using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Trust.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Access.Session;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Trust.Access.Session;

[Authorize]
[ApiController]
[Route("api/trust/access/session")]
[ApiExplorerSettings(GroupName = "trust.access.session")]
public sealed class SessionController : TrustControllerBase
{
    private static readonly DomainRoute SessionRoute = new("trust", "access", "session");

    public SessionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("open")]
    public Task<IActionResult> Open([FromBody] ApiRequest<OpenSessionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var sessionId = IdGenerator.Generate($"trust:access:session:{p.IdentityReference}:{p.SessionContext}:{p.SessionNonce}");
        var cmd = new OpenSessionCommand(sessionId, p.IdentityReference, p.SessionContext, Clock.UtcNow);
        return Dispatch(cmd, SessionRoute, "session_opened", "trust.access.session.open_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<SessionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireSessionCommand(request.Data.SessionId);
        return Dispatch(cmd, SessionRoute, "session_expired", "trust.access.session.expire_failed", ct);
    }

    [HttpPost("terminate")]
    public Task<IActionResult> Terminate([FromBody] ApiRequest<SessionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new TerminateSessionCommand(request.Data.SessionId);
        return Dispatch(cmd, SessionRoute, "session_terminated", "trust.access.session.terminate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetSession(Guid id, CancellationToken ct) =>
        LoadReadModel<SessionReadModel>(
            id,
            "projection_trust_access_session",
            "session_read_model",
            "trust.access.session.not_found",
            ct);
}

public sealed record OpenSessionRequestModel(Guid IdentityReference, string SessionContext, string SessionNonce);
public sealed record SessionIdRequestModel(Guid SessionId);
