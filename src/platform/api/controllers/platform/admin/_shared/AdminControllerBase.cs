using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Platform.Api;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Admin.Shared;

/// <summary>
/// R4.B — shared base for every controller under the
/// <see cref="AdminScope.RoutePrefix"/> surface. Centralises:
///
/// <list type="bullet">
///   <item>admin policy gating (<see cref="AuthorizeAttribute"/> with
///   <see cref="AdminScope.PolicyName"/>),</item>
///   <item>operator identity + correlation id capture,</item>
///   <item>the <c>ApiResponse.Ok/Fail</c> envelope shape the rest of the
///   API uses,</item>
///   <item>a mandatory audit-emission helper so every operator action's
///   refused/failed path writes evidence even when the controller short-
///   circuits on a precondition check.</item>
/// </list>
///
/// <para>Admin controllers MUST NOT reference domain aggregate types
/// directly (enforced by
/// <c>AdminSurfaceArchitectureTests.Admin_controllers_never_reference_domain_aggregates</c>).
/// Every mutation flows through a sanctioned runtime service:
/// <see cref="ISystemIntentDispatcher"/>,
/// <see cref="IDeadLetterRedriveService"/>, or the outbound-effect finality
/// service.</para>
/// </summary>
[Authorize(Policy = AdminScope.PolicyName)]
[ApiController]
public abstract class AdminControllerBase : ControllerBase
{
    protected ICallerIdentityAccessor CallerIdentity { get; }
    protected IOperatorActionRecorder AuditRecorder { get; }
    protected IClock Clock { get; }

    protected AdminControllerBase(
        ICallerIdentityAccessor callerIdentity,
        IOperatorActionRecorder auditRecorder,
        IClock clock)
    {
        CallerIdentity = callerIdentity;
        AuditRecorder = auditRecorder;
        Clock = clock;
    }

    protected string OperatorIdentityId() => CallerIdentity.GetActorId();
    protected string OperatorTenantId() => CallerIdentity.GetTenantId();

    /// <summary>
    /// R4.B / R-ADMIN-AUDIT-EMISSION-01 — every operator action, whether it
    /// succeeded, was refused, or failed, MUST route through this helper so
    /// the audit recorder receives the matching outcome classification.
    /// </summary>
    protected Task<OperatorActionEvent> AuditAsync(
        string actionType,
        Guid targetId,
        string targetResourceType,
        string outcome,
        string? rationale = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default) =>
        AuditRecorder.RecordAsync(
            actionType,
            targetId,
            targetResourceType,
            OperatorIdentityId(),
            OperatorTenantId(),
            this.RequestCorrelationId(),
            outcome,
            rationale,
            failureReason,
            cancellationToken);

    protected IActionResult Ok<T>(T data) =>
        base.Ok(ApiResponse.Ok(data, this.RequestCorrelationId(), Clock.UtcNow));

    protected IActionResult Refused(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        var envelope = ApiResponse.Fail(code, message, Clock.UtcNow, this.RequestCorrelationId());
        return statusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(envelope),
            StatusCodes.Status409Conflict => Conflict(envelope),
            _ => BadRequest(envelope),
        };
    }
}
