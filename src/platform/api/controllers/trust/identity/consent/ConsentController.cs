using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Trust.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Trust.Identity.Consent;

[Authorize]
[ApiController]
[Route("api/trust/identity/consent")]
[ApiExplorerSettings(GroupName = "trust.identity.consent")]
public sealed class ConsentController : TrustControllerBase
{
    private static readonly DomainRoute ConsentRoute = new("trust", "identity", "consent");

    public ConsentController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("grant")]
    public Task<IActionResult> Grant([FromBody] ApiRequest<GrantConsentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var consentId = IdGenerator.Generate($"trust:identity:consent:{p.IdentityReference}:{p.ConsentScope}");
        var cmd = new GrantConsentCommand(consentId, p.IdentityReference, p.ConsentScope, p.ConsentPurpose, Clock.UtcNow);
        return Dispatch(cmd, ConsentRoute, "consent_granted", "trust.identity.consent.grant_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<ConsentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeConsentCommand(request.Data.ConsentId);
        return Dispatch(cmd, ConsentRoute, "consent_revoked", "trust.identity.consent.revoke_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ConsentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireConsentCommand(request.Data.ConsentId);
        return Dispatch(cmd, ConsentRoute, "consent_expired", "trust.identity.consent.expire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetConsent(Guid id, CancellationToken ct) =>
        LoadReadModel<ConsentReadModel>(
            id,
            "projection_trust_identity_consent",
            "consent_read_model",
            "trust.identity.consent.not_found",
            ct);
}

public sealed record GrantConsentRequestModel(Guid IdentityReference, string ConsentScope, string ConsentPurpose);
public sealed record ConsentIdRequestModel(Guid ConsentId);
