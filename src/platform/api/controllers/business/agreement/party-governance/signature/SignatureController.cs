using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.PartyGovernance.Signature;

[Authorize]
[ApiController]
[Route("api/party-governance/signature")]
[ApiExplorerSettings(GroupName = "business.agreement.party-governance.signature")]
public sealed class SignatureController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute SignatureRoute = new("business", "agreement", "signature");

    public SignatureController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<SignatureIdRequestModel> request, CancellationToken ct)
    {
        // SignatureId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01 — no
        // clock/Random entropy on the API hot path).
        var cmd = new CreateSignatureCommand(request.Data.SignatureId);
        return Dispatch(cmd, SignatureRoute, "signature_created", "business.agreement.signature.create_failed", ct);
    }

    [HttpPost("sign")]
    public Task<IActionResult> Sign([FromBody] ApiRequest<SignatureIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SignSignatureCommand(request.Data.SignatureId);
        return Dispatch(cmd, SignatureRoute, "signature_signed", "business.agreement.signature.sign_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<SignatureIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeSignatureCommand(request.Data.SignatureId);
        return Dispatch(cmd, SignatureRoute, "signature_revoked", "business.agreement.signature.revoke_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetSignature(Guid id, CancellationToken ct) =>
        LoadReadModel<SignatureReadModel>(
            id,
            "projection_business_agreement_signature",
            "signature_read_model",
            "business.agreement.signature.not_found",
            ct);
}

public sealed record SignatureIdRequestModel(Guid SignatureId);
