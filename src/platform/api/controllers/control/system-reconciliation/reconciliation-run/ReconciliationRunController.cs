using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemReconciliation.ReconciliationRun;

[Authorize]
[ApiController]
[Route("api/control/reconciliation-run")]
[ApiExplorerSettings(GroupName = "control.system-reconciliation.reconciliation-run")]
public sealed class ReconciliationRunController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-reconciliation", "reconciliation-run");

    public ReconciliationRunController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("schedule")]
    public Task<IActionResult> Schedule([FromBody] ApiRequest<ScheduleReconciliationRunRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ScheduleReconciliationRunCommand(p.RunId, p.Scope);
        return Dispatch(cmd, Route, "reconciliation_run_scheduled", "control.system-reconciliation.reconciliation-run.schedule_failed", ct);
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartReconciliationRunRequestModel> request, CancellationToken ct)
    {
        var cmd = new StartReconciliationRunCommand(request.Data.RunId, request.Data.StartedAt);
        return Dispatch(cmd, Route, "reconciliation_run_started", "control.system-reconciliation.reconciliation-run.start_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteReconciliationRunRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteReconciliationRunCommand(p.RunId, p.ChecksProcessed, p.DiscrepanciesFound, p.CompletedAt);
        return Dispatch(cmd, Route, "reconciliation_run_completed", "control.system-reconciliation.reconciliation-run.complete_failed", ct);
    }

    [HttpPost("abort")]
    public Task<IActionResult> Abort([FromBody] ApiRequest<AbortReconciliationRunRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AbortReconciliationRunCommand(p.RunId, p.Reason, p.AbortedAt);
        return Dispatch(cmd, Route, "reconciliation_run_aborted", "control.system-reconciliation.reconciliation-run.abort_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ReconciliationRunReadModel>(
            id,
            "projection_control_system_reconciliation_reconciliation_run",
            "reconciliation_run_read_model",
            "control.system-reconciliation.reconciliation-run.not_found",
            ct);
}

public sealed record ScheduleReconciliationRunRequestModel(Guid RunId, string Scope);
public sealed record StartReconciliationRunRequestModel(Guid RunId, DateTimeOffset StartedAt);

public sealed record CompleteReconciliationRunRequestModel(
    Guid RunId,
    int ChecksProcessed,
    int DiscrepanciesFound,
    DateTimeOffset CompletedAt);

public sealed record AbortReconciliationRunRequestModel(
    Guid RunId,
    string Reason,
    DateTimeOffset AbortedAt);
