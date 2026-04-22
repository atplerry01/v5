using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Trust.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Trust.Identity.Verification;

[Authorize]
[ApiController]
[Route("api/trust/identity/verification")]
[ApiExplorerSettings(GroupName = "trust.identity.verification")]
public sealed class VerificationController : TrustControllerBase
{
    private static readonly DomainRoute VerificationRoute = new("trust", "identity", "verification");

    public VerificationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("initiate")]
    public Task<IActionResult> Initiate([FromBody] ApiRequest<InitiateVerificationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var verificationId = IdGenerator.Generate($"trust:identity:verification:{p.IdentityReference}:{p.ClaimType}");
        var cmd = new InitiateVerificationCommand(verificationId, p.IdentityReference, p.ClaimType, Clock.UtcNow);
        return Dispatch(cmd, VerificationRoute, "verification_initiated", "trust.identity.verification.initiate_failed", ct);
    }

    [HttpPost("pass")]
    public Task<IActionResult> Pass([FromBody] ApiRequest<VerificationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new PassVerificationCommand(request.Data.VerificationId);
        return Dispatch(cmd, VerificationRoute, "verification_passed", "trust.identity.verification.pass_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<VerificationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new FailVerificationCommand(request.Data.VerificationId);
        return Dispatch(cmd, VerificationRoute, "verification_failed", "trust.identity.verification.fail_failed", ct);
    }

    [HttpGet("{verificationId:guid}")]
    public Task<IActionResult> Get(Guid verificationId, CancellationToken ct)
        => LoadReadModel<VerificationReadModel>(verificationId, "projection_trust_identity_verification", "verification_read_model", "trust.identity.verification.not_found", ct);
}

public sealed record InitiateVerificationRequestModel(Guid IdentityReference, string ClaimType);
public sealed record VerificationIdRequestModel(Guid VerificationId);
