using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Entitlement.EligibilityAndGrant.Grant;

[Authorize]
[ApiController]
[Route("api/eligibility-and-grant/grant")]
[ApiExplorerSettings(GroupName = "business.entitlement.eligibility-and-grant.grant")]
public sealed class GrantController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute GrantRoute = new("business", "entitlement", "grant");

    public GrantController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateGrantRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // GrantId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01).
        var cmd = new CreateGrantCommand(p.GrantId, p.SubjectId, p.TargetId, p.Scope, Clock.UtcNow, p.ExpiresAt);
        return Dispatch(cmd, GrantRoute, "grant_created", "business.entitlement.grant.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<GrantIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateGrantCommand(request.Data.GrantId, Clock.UtcNow);
        return Dispatch(cmd, GrantRoute, "grant_activated", "business.entitlement.grant.activate_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<GrantIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeGrantCommand(request.Data.GrantId, Clock.UtcNow);
        return Dispatch(cmd, GrantRoute, "grant_revoked", "business.entitlement.grant.revoke_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<GrantIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireGrantCommand(request.Data.GrantId, Clock.UtcNow);
        return Dispatch(cmd, GrantRoute, "grant_expired", "business.entitlement.grant.expire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetGrant(Guid id, CancellationToken ct) =>
        LoadReadModel<GrantReadModel>(
            id,
            "projection_business_entitlement_grant",
            "grant_read_model",
            "business.entitlement.grant.not_found",
            ct);
}

public sealed record CreateGrantRequestModel(Guid GrantId, Guid SubjectId, Guid TargetId, string Scope, DateTimeOffset? ExpiresAt);
public sealed record GrantIdRequestModel(Guid GrantId);
