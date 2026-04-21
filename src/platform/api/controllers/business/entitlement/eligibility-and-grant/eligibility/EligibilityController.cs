using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Entitlement.EligibilityAndGrant.Eligibility;

[Authorize]
[ApiController]
[Route("api/eligibility-and-grant/eligibility")]
[ApiExplorerSettings(GroupName = "business.entitlement.eligibility-and-grant.eligibility")]
public sealed class EligibilityController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute EligibilityRoute = new("business", "entitlement", "eligibility");

    public EligibilityController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateEligibilityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // EligibilityId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01).
        var cmd = new CreateEligibilityCommand(p.EligibilityId, p.SubjectId, p.TargetId, p.Scope);
        return Dispatch(cmd, EligibilityRoute, "eligibility_created", "business.entitlement.eligibility.create_failed", ct);
    }

    [HttpPost("mark-eligible")]
    public Task<IActionResult> MarkEligible([FromBody] ApiRequest<EligibilityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new MarkEligibleCommand(request.Data.EligibilityId, Clock.UtcNow);
        return Dispatch(cmd, EligibilityRoute, "eligibility_evaluated_eligible", "business.entitlement.eligibility.mark_eligible_failed", ct);
    }

    [HttpPost("mark-ineligible")]
    public Task<IActionResult> MarkIneligible([FromBody] ApiRequest<MarkIneligibleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new MarkIneligibleCommand(p.EligibilityId, p.Reason, Clock.UtcNow);
        return Dispatch(cmd, EligibilityRoute, "eligibility_evaluated_ineligible", "business.entitlement.eligibility.mark_ineligible_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetEligibility(Guid id, CancellationToken ct) =>
        LoadReadModel<EligibilityReadModel>(
            id,
            "projection_business_entitlement_eligibility",
            "eligibility_read_model",
            "business.entitlement.eligibility.not_found",
            ct);
}

public sealed record CreateEligibilityRequestModel(Guid EligibilityId, Guid SubjectId, Guid TargetId, string Scope);
public sealed record EligibilityIdRequestModel(Guid EligibilityId);
public sealed record MarkIneligibleRequestModel(Guid EligibilityId, string Reason);
