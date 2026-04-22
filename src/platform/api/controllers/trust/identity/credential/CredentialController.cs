using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Trust.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Trust.Identity.Credential;

[Authorize]
[ApiController]
[Route("api/trust/identity/credential")]
[ApiExplorerSettings(GroupName = "trust.identity.credential")]
public sealed class CredentialController : TrustControllerBase
{
    private static readonly DomainRoute CredentialRoute = new("trust", "identity", "credential");

    public CredentialController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("issue")]
    public Task<IActionResult> Issue([FromBody] ApiRequest<IssueCredentialRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var credentialId = IdGenerator.Generate($"trust:identity:credential:{p.IdentityReference}:{p.CredentialType}");
        var cmd = new IssueCredentialCommand(credentialId, p.IdentityReference, p.CredentialType, Clock.UtcNow);
        return Dispatch(cmd, CredentialRoute, "credential_issued", "trust.identity.credential.issue_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<CredentialIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateCredentialCommand(request.Data.CredentialId);
        return Dispatch(cmd, CredentialRoute, "credential_activated", "trust.identity.credential.activate_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<CredentialIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeCredentialCommand(request.Data.CredentialId);
        return Dispatch(cmd, CredentialRoute, "credential_revoked", "trust.identity.credential.revoke_failed", ct);
    }

    [HttpGet("{credentialId:guid}")]
    public Task<IActionResult> Get(Guid credentialId, CancellationToken ct)
        => LoadReadModel<CredentialReadModel>(credentialId, "projection_trust_identity_credential", "credential_read_model", "trust.identity.credential.not_found", ct);
}

public sealed record IssueCredentialRequestModel(Guid IdentityReference, string CredentialType);
public sealed record CredentialIdRequestModel(Guid CredentialId);
