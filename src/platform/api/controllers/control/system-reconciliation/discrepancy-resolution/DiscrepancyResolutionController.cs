using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemReconciliation.DiscrepancyResolution;

[Authorize]
[ApiController]
[Route("api/control/discrepancy-resolution")]
[ApiExplorerSettings(GroupName = "control.system-reconciliation.discrepancy-resolution")]
public sealed class DiscrepancyResolutionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-reconciliation", "discrepancy-resolution");

    public DiscrepancyResolutionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("initiate")]
    public Task<IActionResult> Initiate([FromBody] ApiRequest<InitiateDiscrepancyResolutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new InitiateDiscrepancyResolutionCommand(p.ResolutionId, p.DetectionId, p.InitiatedAt);
        return Dispatch(cmd, Route, "discrepancy_resolution_initiated", "control.system-reconciliation.discrepancy-resolution.initiate_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteDiscrepancyResolutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteDiscrepancyResolutionCommand(p.ResolutionId, p.Outcome, p.Notes, p.CompletedAt);
        return Dispatch(cmd, Route, "discrepancy_resolution_completed", "control.system-reconciliation.discrepancy-resolution.complete_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<DiscrepancyResolutionReadModel>(
            id,
            "projection_control_system_reconciliation_discrepancy_resolution",
            "discrepancy_resolution_read_model",
            "control.system-reconciliation.discrepancy-resolution.not_found",
            ct);
}

public sealed record InitiateDiscrepancyResolutionRequestModel(
    Guid ResolutionId,
    string DetectionId,
    DateTimeOffset InitiatedAt);

public sealed record CompleteDiscrepancyResolutionRequestModel(
    Guid ResolutionId,
    string Outcome,
    string Notes,
    DateTimeOffset CompletedAt);
