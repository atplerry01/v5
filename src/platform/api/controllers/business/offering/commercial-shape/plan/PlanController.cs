using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CommercialShape.Plan;

[Authorize]
[ApiController]
[Route("api/commercial-shape/plan")]
[ApiExplorerSettings(GroupName = "business.offering.commercial-shape.plan")]
public sealed class PlanController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute PlanRoute = new("business", "offering", "plan");

    public PlanController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("draft")]
    public Task<IActionResult> Draft([FromBody] ApiRequest<DraftPlanRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // PlanId MUST be supplied by the caller per DET-SEED-DERIVATION-01 —
        // no clock/Random entropy on the API hot path.
        var cmd = new DraftPlanCommand(p.PlanId, p.PlanName, p.PlanTier);
        return Dispatch(cmd, PlanRoute, "plan_drafted", "business.offering.plan.draft_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<PlanIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivatePlanCommand(request.Data.PlanId);
        return Dispatch(cmd, PlanRoute, "plan_activated", "business.offering.plan.activate_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<PlanIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecatePlanCommand(request.Data.PlanId);
        return Dispatch(cmd, PlanRoute, "plan_deprecated", "business.offering.plan.deprecate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<PlanIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchivePlanCommand(request.Data.PlanId);
        return Dispatch(cmd, PlanRoute, "plan_archived", "business.offering.plan.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetPlan(Guid id, CancellationToken ct) =>
        LoadReadModel<PlanReadModel>(
            id,
            "projection_business_offering_plan",
            "plan_read_model",
            "business.offering.plan.not_found",
            ct);
}

public sealed record DraftPlanRequestModel(Guid PlanId, string PlanName, string PlanTier);
public sealed record PlanIdRequestModel(Guid PlanId);
