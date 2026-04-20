using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Platform.Api.Controllers.Platform.Admin.Shared;
using Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Admin.Workflow;

/// <summary>
/// R4.B — admin inspection + lifecycle-control surface for workflow
/// executions. Inspection paths read the projection store; lifecycle actions
/// (resume, approve, reject) flow through <see cref="ISystemIntentDispatcher"/>
/// using the existing sanctioned commands
/// (<see cref="WorkflowResumeCommand"/>, <see cref="ApproveWorkflowCommand"/>,
/// <see cref="RejectWorkflowCommand"/>). The dispatcher is the canonical
/// gate for policy + aggregate preconditions — the controller never touches
/// the execution aggregate directly.
///
/// <para><b>Pause:</b> intentionally NOT exposed. The workflow runtime does
/// not offer a real operator-pause seam today (suspension is engine-driven);
/// surfacing a fake pause endpoint would violate R4.B's "no invented
/// pause/resume for components that do not truly support it" constraint.
/// When a canonical pause command is added to the workflow lifecycle, the
/// endpoint slots in alongside the existing three.</para>
/// </summary>
[ApiExplorerSettings(GroupName = "platform.admin.workflow")]
[Route(AdminScope.RoutePrefix + "/workflow")]
public sealed class WorkflowAdminController : AdminControllerBase
{
    public const string ResumeActionType = "workflow.resume";
    public const string ApproveActionType = "workflow.approve";
    public const string RejectActionType = "workflow.reject";
    public const string ResourceType = "workflow-execution";

    private static readonly DomainRoute WorkflowRoute = new("orchestration", "workflow", "execution");

    private readonly IWorkflowExecutionProjectionStore _projection;
    private readonly ISystemIntentDispatcher _dispatcher;

    public WorkflowAdminController(
        IWorkflowExecutionProjectionStore projection,
        ISystemIntentDispatcher dispatcher,
        ICallerIdentityAccessor callerIdentity,
        IOperatorActionRecorder auditRecorder,
        IClock clock) : base(callerIdentity, auditRecorder, clock)
    {
        _projection = projection;
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] string? workflowName,
        [FromQuery] string? approvalState,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var results = await _projection.ListAsync(status, workflowName, approvalState, limit, ct);
        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var model = await _projection.GetAsync(id);
        return model is null
            ? Refused("platform.admin.workflow.not_found", $"WorkflowExecution {id} not found.", StatusCodes.Status404NotFound)
            : Ok(model);
    }

    [HttpGet("awaiting-approval")]
    public async Task<IActionResult> ListAwaitingApproval(
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var results = await _projection.ListAsync(
            status: "Suspended", workflowName: null, approvalState: "AwaitingApproval", limit, ct);
        return Ok(results);
    }

    [HttpPost("{id:guid}/resume")]
    public Task<IActionResult> Resume(Guid id, [FromBody] ResumeWorkflowRequest request, CancellationToken ct) =>
        DispatchLifecycleAsync(ResumeActionType, id, request.Rationale,
            new WorkflowResumeCommand(id), ct);

    [HttpPost("{id:guid}/approve")]
    public Task<IActionResult> Approve(Guid id, [FromBody] ApproveWorkflowAdminRequest request, CancellationToken ct) =>
        DispatchLifecycleAsync(ApproveActionType, id, request.Rationale,
            new ApproveWorkflowCommand(id, request.Decision), ct);

    [HttpPost("{id:guid}/reject")]
    public Task<IActionResult> Reject(Guid id, [FromBody] RejectWorkflowAdminRequest request, CancellationToken ct) =>
        DispatchLifecycleAsync(RejectActionType, id, request.Rationale,
            new RejectWorkflowCommand(id, request.Decision), ct);

    private async Task<IActionResult> DispatchLifecycleAsync(
        string actionType,
        Guid targetId,
        string? rationale,
        object command,
        CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, WorkflowRoute, ct);
        if (!result.IsSuccess)
        {
            await AuditAsync(actionType, targetId, ResourceType,
                OperatorActionOutcomes.Refused, rationale,
                failureReason: result.Error ?? "dispatch_failed", ct);
            return Refused($"platform.admin.{actionType}.dispatch_failed",
                result.Error ?? "Command dispatch was rejected.");
        }

        var audit = await AuditAsync(actionType, targetId, ResourceType,
            OperatorActionOutcomes.Accepted, rationale,
            failureReason: null, ct);

        return Ok(new WorkflowAdminActionResponse(
            WorkflowId: targetId,
            Action: actionType,
            CorrelationId: result.CorrelationId,
            AuditEventId: audit.EventId));
    }
}

public sealed record ResumeWorkflowRequest(string? Rationale);
public sealed record ApproveWorkflowAdminRequest(ApprovalDecisionPayload Decision, string? Rationale);
public sealed record RejectWorkflowAdminRequest(ApprovalDecisionPayload Decision, string? Rationale);

public sealed record WorkflowAdminActionResponse(
    Guid WorkflowId,
    string Action,
    Guid CorrelationId,
    Guid AuditEventId);
