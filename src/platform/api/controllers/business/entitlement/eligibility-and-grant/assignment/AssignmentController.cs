using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Entitlement.EligibilityAndGrant.Assignment;

[Authorize]
[ApiController]
[Route("api/eligibility-and-grant/assignment")]
[ApiExplorerSettings(GroupName = "business.entitlement.eligibility-and-grant.assignment")]
public sealed class AssignmentController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute AssignmentRoute = new("business", "entitlement", "assignment");

    public AssignmentController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateAssignmentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // AssignmentId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01).
        var cmd = new CreateAssignmentCommand(p.AssignmentId, p.GrantId, p.SubjectId, p.Scope);
        return Dispatch(cmd, AssignmentRoute, "assignment_created", "business.entitlement.assignment.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<AssignmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateAssignmentCommand(request.Data.AssignmentId, Clock.UtcNow);
        return Dispatch(cmd, AssignmentRoute, "assignment_activated", "business.entitlement.assignment.activate_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<AssignmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeAssignmentCommand(request.Data.AssignmentId, Clock.UtcNow);
        return Dispatch(cmd, AssignmentRoute, "assignment_revoked", "business.entitlement.assignment.revoke_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAssignment(Guid id, CancellationToken ct) =>
        LoadReadModel<AssignmentReadModel>(
            id,
            "projection_business_entitlement_assignment",
            "assignment_read_model",
            "business.entitlement.assignment.not_found",
            ct);
}

public sealed record CreateAssignmentRequestModel(Guid AssignmentId, Guid GrantId, Guid SubjectId, string Scope);
public sealed record AssignmentIdRequestModel(Guid AssignmentId);
