using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Platform.Api.Controllers.Platform.Admin.Shared;
using Whycespace.Shared.Contracts.Projections.Integration.OutboundEffect;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Admin.OutboundEffects;

/// <summary>
/// R4.B — admin inspection + reconciliation surface for outbound effects.
/// Inspection paths are read-only reads from the projection store; the
/// reconcile path delegates to <see cref="IOutboundEffectFinalityService.ReconcileAsync"/>
/// which owns the precondition gate (rejects anything that is not currently
/// in <c>ReconciliationRequired</c>).
/// </summary>
[ApiExplorerSettings(GroupName = "platform.admin.outbound-effects")]
[Route(AdminScope.RoutePrefix + "/outbound-effects")]
public sealed class OutboundEffectAdminController : AdminControllerBase
{
    public const string ReconcileActionType = "outbound-effect.reconcile";
    public const string ResourceType = "outbound-effect";

    private readonly IOutboundEffectProjectionStore _projection;
    private readonly IOutboundEffectFinalityService _finality;

    public OutboundEffectAdminController(
        IOutboundEffectProjectionStore projection,
        IOutboundEffectFinalityService finality,
        ICallerIdentityAccessor callerIdentity,
        IOperatorActionRecorder auditRecorder,
        IClock clock) : base(callerIdentity, auditRecorder, clock)
    {
        _projection = projection;
        _finality = finality;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? providerId,
        [FromQuery] string? effectType,
        [FromQuery] string? status,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var results = await _projection.ListAsync(providerId, effectType, status, limit, ct);
        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var model = await _projection.GetAsync(id);
        return model is null
            ? Refused("platform.admin.outbound_effect.not_found", $"OutboundEffect {id} not found.", StatusCodes.Status404NotFound)
            : Ok(model);
    }

    [HttpGet("reconciliation-required")]
    public async Task<IActionResult> ListReconciliationRequired(
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var results = await _projection.ListByStatusAsync("ReconciliationRequired", limit, ct);
        return Ok(results);
    }

    [HttpPost("{id:guid}/reconcile")]
    public async Task<IActionResult> Reconcile(
        Guid id,
        [FromBody] ReconcileOutboundEffectRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceDigest))
        {
            await AuditAsync(ReconcileActionType, id, ResourceType,
                OperatorActionOutcomes.Refused, request.Rationale,
                failureReason: "EvidenceDigest is required.", ct);
            return Refused("platform.admin.outbound_effect.reconcile.invalid_evidence",
                "EvidenceDigest is required.");
        }

        try
        {
            await _finality.ReconcileAsync(
                id, request.Outcome, request.EvidenceDigest, OperatorIdentityId(), ct);
        }
        catch (InvalidOperationException ex)
        {
            await AuditAsync(ReconcileActionType, id, ResourceType,
                OperatorActionOutcomes.Refused, request.Rationale,
                failureReason: ex.Message, ct);
            return Refused("platform.admin.outbound_effect.reconcile.precondition_failed", ex.Message);
        }

        var audit = await AuditAsync(ReconcileActionType, id, ResourceType,
            OperatorActionOutcomes.Accepted, request.Rationale,
            failureReason: null, ct);

        return Ok(new ReconcileOutboundEffectResponse(
            EffectId: id,
            Outcome: request.Outcome.ToString(),
            AuditEventId: audit.EventId));
    }
}

public sealed record ReconcileOutboundEffectRequest(
    OutboundFinalityOutcome Outcome,
    string EvidenceDigest,
    string? Rationale);

public sealed record ReconcileOutboundEffectResponse(
    Guid EffectId,
    string Outcome,
    Guid AuditEventId);
