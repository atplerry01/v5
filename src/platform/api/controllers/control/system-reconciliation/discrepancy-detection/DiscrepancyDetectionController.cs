using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemReconciliation.DiscrepancyDetection;

[Authorize]
[ApiController]
[Route("api/control/discrepancy-detection")]
[ApiExplorerSettings(GroupName = "control.system-reconciliation.discrepancy-detection")]
public sealed class DiscrepancyDetectionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-reconciliation", "discrepancy-detection");

    public DiscrepancyDetectionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("detect")]
    public Task<IActionResult> Detect([FromBody] ApiRequest<DetectDiscrepancyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DetectDiscrepancyCommand(p.DetectionId, p.Kind, p.SourceReference, p.DetectedAt);
        return Dispatch(cmd, Route, "discrepancy_detected", "control.system-reconciliation.discrepancy-detection.detect_failed", ct);
    }

    [HttpPost("dismiss")]
    public Task<IActionResult> Dismiss([FromBody] ApiRequest<DismissDiscrepancyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DismissDiscrepancyCommand(p.DetectionId, p.Reason, p.DismissedAt);
        return Dispatch(cmd, Route, "discrepancy_dismissed", "control.system-reconciliation.discrepancy-detection.dismiss_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<DiscrepancyDetectionReadModel>(
            id,
            "projection_control_system_reconciliation_discrepancy_detection",
            "discrepancy_detection_read_model",
            "control.system-reconciliation.discrepancy-detection.not_found",
            ct);
}

public sealed record DetectDiscrepancyRequestModel(
    Guid DetectionId,
    string Kind,
    string SourceReference,
    DateTimeOffset DetectedAt);

public sealed record DismissDiscrepancyRequestModel(
    Guid DetectionId,
    string Reason,
    DateTimeOffset DismissedAt);
